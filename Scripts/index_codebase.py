"""
🚀 PINECONE CODEBASE INDEXER
Index toàn bộ source code vào Pinecone Vector Database

Requirements:
    pip install openai pinecone-client python-dotenv

Usage:
    python index_codebase.py
"""

import os
import json
from pathlib import Path
from typing import List, Dict
import openai
from pinecone import Pinecone, ServerlessSpec
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

# Configuration
PINECONE_API_KEY = os.getenv("PINECONE_API_KEY", "YOUR_PINECONE_API_KEY")
PINECONE_ENVIRONMENT = os.getenv("PINECONE_ENVIRONMENT", "us-east-1-aws")
PINECONE_INDEX_NAME = os.getenv("PINECONE_INDEX_NAME", "sms-codebase")
OPENAI_API_KEY = os.getenv("OPENAI_API_KEY", "YOUR_OPENAI_API_KEY")

# File extensions to index
CODE_EXTENSIONS = {
    '.cs', '.ts', '.js', '.html', '.css', '.scss', 
    '.json', '.sql', '.md', '.txt'
}

# Folders to ignore
IGNORE_FOLDERS = {
    'node_modules', 'bin', 'obj', '.git', '.vs', 
    'wwwroot/lib', 'ClientApp/.angular', 'dist'
}

def should_index_file(file_path: Path) -> bool:
    """Check if file should be indexed"""
    # Check extension
    if file_path.suffix not in CODE_EXTENSIONS:
        return False
    
    # Check if in ignored folder
    for part in file_path.parts:
        if part in IGNORE_FOLDERS:
            return False
    
    # Check file size (skip if > 100KB)
    if file_path.stat().st_size > 100 * 1024:
        return False
    
    return True

def chunk_code(content: str, file_path: str, chunk_size: int = 1000) -> List[Dict]:
    """Split code into chunks for better retrieval"""
    chunks = []
    lines = content.split('\n')
    
    current_chunk = []
    current_size = 0
    start_line = 1
    
    for i, line in enumerate(lines, 1):
        line_size = len(line)
        
        if current_size + line_size > chunk_size and current_chunk:
            # Save current chunk
            chunk_text = '\n'.join(current_chunk)
            chunks.append({
                'text': chunk_text,
                'metadata': {
                    'file_path': file_path,
                    'start_line': start_line,
                    'end_line': i - 1,
                    'chunk_size': current_size
                }
            })
            
            # Start new chunk
            current_chunk = [line]
            current_size = line_size
            start_line = i
        else:
            current_chunk.append(line)
            current_size += line_size
    
    # Save last chunk
    if current_chunk:
        chunk_text = '\n'.join(current_chunk)
        chunks.append({
            'text': chunk_text,
            'metadata': {
                'file_path': file_path,
                'start_line': start_line,
                'end_line': len(lines),
                'chunk_size': current_size
            }
        })
    
    return chunks

def get_embedding(text: str, model: str = "text-embedding-ada-002") -> List[float]:
    """Get OpenAI embedding for text"""
    response = openai.embeddings.create(
        model=model,
        input=text
    )
    return response.data[0].embedding

def index_codebase(root_path: str):
    """Index entire codebase into Pinecone"""
    
    print("🚀 Starting Codebase Indexing...\n")
    
    # Initialize OpenAI
    openai.api_key = OPENAI_API_KEY
    print(f"✅ OpenAI API configured")
    
    # Initialize Pinecone
    pc = Pinecone(api_key=PINECONE_API_KEY)
    
    # Create or connect to index
    if PINECONE_INDEX_NAME not in pc.list_indexes().names():
        print(f"📦 Creating index: {PINECONE_INDEX_NAME}")
        pc.create_index(
            name=PINECONE_INDEX_NAME,
            dimension=1536,  # OpenAI embedding dimension
            metric='cosine',
            spec=ServerlessSpec(
                cloud='aws',
                region=PINECONE_ENVIRONMENT.replace('-aws', '')
            )
        )
        print(f"✅ Index created: {PINECONE_INDEX_NAME}")
    else:
        print(f"✅ Index exists: {PINECONE_INDEX_NAME}")
    
    # Connect to index
    index = pc.Index(PINECONE_INDEX_NAME)
    print(f"✅ Connected to index")
    
    # Get index stats
    stats = index.describe_index_stats()
    print(f"📊 Current vectors in index: {stats.get('total_vector_count', 0)}\n")
    
    # Scan files
    root = Path(root_path)
    files_to_index = []
    
    print("🔍 Scanning files...")
    for file_path in root.rglob('*'):
        if file_path.is_file() and should_index_file(file_path):
            files_to_index.append(file_path)
    
    print(f"📁 Found {len(files_to_index)} files to index\n")
    
    # Index files
    total_chunks = 0
    vectors_batch = []
    batch_size = 100
    
    for i, file_path in enumerate(files_to_index, 1):
        try:
            # Read file
            relative_path = str(file_path.relative_to(root))
            print(f"[{i}/{len(files_to_index)}] Processing: {relative_path}")
            
            with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
                content = f.read()
            
            # Skip empty files
            if not content.strip():
                print(f"  ⚠️  Skipped (empty)")
                continue
            
            # Chunk code
            chunks = chunk_code(content, relative_path)
            print(f"  📄 Chunks: {len(chunks)}")
            
            # Generate embeddings and prepare vectors
            for j, chunk in enumerate(chunks):
                try:
                    # Get embedding
                    embedding = get_embedding(chunk['text'])
                    
                    # Prepare vector
                    vector_id = f"{relative_path}::chunk_{j}"
                    vector = {
                        'id': vector_id,
                        'values': embedding,
                        'metadata': {
                            'file_name': file_path.name,
                            'file_path': relative_path,
                            'file_type': file_path.suffix[1:],  # Remove dot
                            'content': chunk['text'][:1000],  # Limit metadata size
                            'start_line': chunk['metadata']['start_line'],
                            'end_line': chunk['metadata']['end_line'],
                            'chunk_index': j,
                            'total_chunks': len(chunks)
                        }
                    }
                    
                    vectors_batch.append(vector)
                    total_chunks += 1
                    
                    # Upsert batch if full
                    if len(vectors_batch) >= batch_size:
                        index.upsert(vectors=vectors_batch)
                        print(f"  ✅ Upserted {len(vectors_batch)} vectors")
                        vectors_batch = []
                
                except Exception as e:
                    print(f"  ❌ Error embedding chunk {j}: {e}")
            
        except Exception as e:
            print(f"  ❌ Error processing file: {e}")
    
    # Upsert remaining vectors
    if vectors_batch:
        index.upsert(vectors=vectors_batch)
        print(f"  ✅ Upserted final {len(vectors_batch)} vectors")
    
    # Final stats
    print(f"\n{'='*60}")
    print(f"✅ INDEXING COMPLETE!")
    print(f"{'='*60}")
    print(f"📁 Files processed: {len(files_to_index)}")
    print(f"📄 Total chunks: {total_chunks}")
    
    # Get updated stats
    stats = index.describe_index_stats()
    print(f"📊 Vectors in index: {stats.get('total_vector_count', 0)}")
    print(f"\n🎉 Your codebase is now searchable with AI!")

if __name__ == "__main__":
    # Get project root (2 levels up from script)
    script_dir = Path(__file__).parent
    project_root = script_dir
    
    print(f"📂 Project root: {project_root}\n")
    
    # Check API keys
    if PINECONE_API_KEY == "YOUR_PINECONE_API_KEY":
        print("❌ Error: PINECONE_API_KEY not set!")
        print("Create .env file with:")
        print("  PINECONE_API_KEY=your_key_here")
        print("  OPENAI_API_KEY=your_key_here")
        exit(1)
    
    if OPENAI_API_KEY == "YOUR_OPENAI_API_KEY":
        print("❌ Error: OPENAI_API_KEY not set!")
        print("Create .env file with:")
        print("  OPENAI_API_KEY=your_key_here")
        exit(1)
    
    # Start indexing
    index_codebase(str(project_root))
