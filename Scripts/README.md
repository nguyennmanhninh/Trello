# ⚙️ Scripts

Folder này chứa các automation scripts.

## 📋 Files

### Database Scripts
- `ImportSampleData.ps1` - **Import database với sample data**
  ```powershell
  .\ImportSampleData.ps1
  ```

### Python Scripts
- `index_codebase.py` - Index codebase vào Pinecone Vector DB
  ```bash
  pip install -r ../requirements.txt
  python index_codebase.py
  ```

- `generate_knowledge_base.py` - Generate knowledge base từ code
  ```bash
  python generate_knowledge_base.py
  ```

- `fix_teachers_template.py` - Fix template issues

### Quick Start
- `run.bat` - **Chạy nhanh cả Backend + Frontend**
  ```cmd
  run.bat
  ```

- `debug.bat` - Chạy ở debug mode
  ```cmd
  debug.bat
  ```

## 🚀 Common Commands

### Chạy Backend
```powershell
dotnet restore
dotnet build
dotnet run
# Backend: http://localhost:5298
```

### Chạy Frontend
```powershell
cd ClientApp
npm install
npm start
# Frontend: http://localhost:4200
```

### Cả hai cùng lúc
```cmd
run.bat
```

## 📦 Dependencies

### Python
```bash
pip install -r requirements.txt
```

### .NET
```bash
dotnet restore
```

### Angular
```bash
cd ClientApp
npm install
```
