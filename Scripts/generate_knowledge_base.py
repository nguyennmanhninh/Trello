# -*- coding: utf-8 -*-
"""
Auto-upload Knowledge Base to Tawk.to
This script reads KNOWLEDGE_BASE.md and formats it for Tawk.to
"""

import re
from typing import List, Dict

class TawkKnowledgeBaseGenerator:
    def __init__(self, markdown_file: str):
        self.markdown_file = markdown_file
        self.categories = []
        self.articles = []
        
    def parse_markdown(self) -> None:
        """Parse markdown file and extract Q&A"""
        with open(self.markdown_file, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Split by main sections (##)
        sections = re.split(r'\n## ', content)
        
        current_category = None
        
        for section in sections[1:]:  # Skip first empty split
            lines = section.split('\n')
            category_name = lines[0].strip().replace('📚', '').replace('👥', '').replace('📖', '').replace('🔐', '').replace('📊', '').replace('🔧', '').replace('❓', '').replace('🐛', '').replace('📞', '').replace('🎓', '').strip()
            
            # Extract Q&A pairs
            qa_pairs = self.extract_qa_pairs('\n'.join(lines[1:]))
            
            if qa_pairs:
                self.categories.append({
                    'name': category_name,
                    'description': f'Câu hỏi về {category_name}',
                    'articles': qa_pairs
                })
    
    def extract_qa_pairs(self, text: str) -> List[Dict[str, str]]:
        """Extract Question-Answer pairs from text"""
        qa_pairs = []
        
        # Find all Q: and A: patterns
        questions = re.findall(r'\*\*Q: (.+?)\?\*\*', text)
        answers = re.findall(r'A:\s*\n(.+?)(?=\n\*\*Q:|$)', text, re.DOTALL)
        
        for q, a in zip(questions, answers):
            qa_pairs.append({
                'title': q.strip() + '?',
                'content': a.strip(),
                'tags': self.extract_tags(q)
            })
        
        return qa_pairs
    
    def extract_tags(self, question: str) -> List[str]:
        """Extract relevant tags from question"""
        tags = []
        
        keywords = {
            'thêm': 'thêm mới',
            'xóa': 'xóa',
            'sửa': 'chỉnh sửa',
            'tìm': 'tìm kiếm',
            'export': 'xuất file',
            'sinh viên': 'sinh viên',
            'giáo viên': 'giáo viên',
            'lớp': 'lớp học',
            'điểm': 'điểm số',
            'môn học': 'môn học',
            'khoa': 'khoa',
            'đăng nhập': 'đăng nhập',
            'mật khẩu': 'mật khẩu'
        }
        
        question_lower = question.lower()
        for keyword, tag in keywords.items():
            if keyword in question_lower:
                tags.append(tag)
        
        return tags if tags else ['hướng dẫn']
    
    def generate_html_output(self) -> str:
        """Generate HTML format for manual copy-paste"""
        html = """
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <title>Tawk.to Knowledge Base Import</title>
    <style>
        body { font-family: Arial, sans-serif; padding: 20px; max-width: 1200px; margin: 0 auto; }
        .category { margin-bottom: 40px; border: 2px solid #ddd; padding: 20px; border-radius: 8px; }
        .category-title { font-size: 24px; color: #2563eb; margin-bottom: 10px; }
        .article { margin-bottom: 20px; background: #f9fafb; padding: 15px; border-radius: 5px; }
        .article-title { font-size: 18px; font-weight: bold; color: #1f2937; margin-bottom: 8px; }
        .article-content { color: #4b5563; line-height: 1.6; white-space: pre-wrap; }
        .tags { margin-top: 8px; }
        .tag { display: inline-block; background: #dbeafe; color: #1e40af; padding: 4px 8px; 
               border-radius: 4px; font-size: 12px; margin-right: 5px; }
        .instructions { background: #fef3c7; padding: 20px; border-radius: 8px; margin-bottom: 30px; }
        .instructions h2 { color: #92400e; margin-top: 0; }
        .instructions ol { color: #78350f; }
    </style>
</head>
<body>
    <div class="instructions">
        <h2>🔧 Hướng dẫn import vào Tawk.to</h2>
        <ol>
            <li>Login vào <a href="https://dashboard.tawk.to/" target="_blank">Tawk.to Dashboard</a></li>
            <li>Vào <strong>Knowledge Base</strong> → <strong>Categories</strong></li>
            <li>Với mỗi Category bên dưới:
                <ul>
                    <li>Click <strong>"Add Category"</strong></li>
                    <li>Paste <strong>Category Title</strong></li>
                    <li>Click <strong>"Add Article"</strong> cho mỗi câu hỏi</li>
                    <li>Paste <strong>Article Title</strong> (câu hỏi)</li>
                    <li>Paste <strong>Article Content</strong> (câu trả lời)</li>
                    <li>Thêm <strong>Tags</strong> từ danh sách</li>
                </ul>
            </li>
            <li>Click <strong>"Publish"</strong></li>
            <li>Vào <strong>Chatbot</strong> → Enable <strong>"Knowledge Base Search"</strong></li>
            <li>Test chatbot: Hỏi bất kỳ câu hỏi nào → Bot sẽ tự động trả lời!</li>
        </ol>
    </div>
"""
        
        for category in self.categories:
            html += f"""
    <div class="category">
        <div class="category-title">📁 {category['name']}</div>
"""
            
            for article in category['articles']:
                html += f"""
        <div class="article">
            <div class="article-title">❓ {article['title']}</div>
            <div class="article-content">{article['content']}</div>
            <div class="tags">
"""
                for tag in article['tags']:
                    html += f'                <span class="tag">{tag}</span>\n'
                
                html += """            </div>
        </div>
"""
            
            html += """    </div>
"""
        
        html += """
</body>
</html>
"""
        return html
    
    def generate_csv_output(self) -> str:
        """Generate CSV format for bulk import (if Tawk.to supports it)"""
        csv = "Category,Question,Answer,Tags\n"
        
        for category in self.categories:
            for article in category['articles']:
                question = article['title'].replace('"', '""')
                answer = article['content'].replace('"', '""')
                tags = '|'.join(article['tags'])
                
                csv += f'"{category["name"]}","{question}","{answer}","{tags}"\n'
        
        return csv
    
    def save_outputs(self) -> None:
        """Save HTML and CSV outputs"""
        # Save HTML
        with open('TAWK_IMPORT.html', 'w', encoding='utf-8') as f:
            f.write(self.generate_html_output())
        
        # Save CSV
        with open('TAWK_IMPORT.csv', 'w', encoding='utf-8') as f:
            f.write(self.generate_csv_output())
        
        print("✅ Generated files:")
        print("   - TAWK_IMPORT.html (Open in browser to view formatted)")
        print("   - TAWK_IMPORT.csv (For bulk import if supported)")
        
        # Print summary
        total_articles = sum(len(cat['articles']) for cat in self.categories)
        print(f"\n📊 Summary:")
        print(f"   - Categories: {len(self.categories)}")
        print(f"   - Total Q&A: {total_articles}")
        
        print("\n🎯 Next steps:")
        print("   1. Open TAWK_IMPORT.html in browser")
        print("   2. Follow instructions to import into Tawk.to")
        print("   3. Enable Knowledge Base Search in Tawk.to Chatbot settings")
        print("   4. Test your chatbot!")

def main():
    print("🤖 Tawk.to Knowledge Base Generator")
    print("=" * 50)
    
    generator = TawkKnowledgeBaseGenerator('KNOWLEDGE_BASE.md')
    
    print("📖 Parsing KNOWLEDGE_BASE.md...")
    generator.parse_markdown()
    
    print("💾 Generating output files...")
    generator.save_outputs()
    
    print("\n✨ Done!")

if __name__ == "__main__":
    main()
