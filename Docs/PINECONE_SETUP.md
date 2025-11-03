# 🚀 PINECONE VECTOR DATABASE SETUP

## 📌 **STEP 1: CREATE FREE PINECONE ACCOUNT**

### 1.1 Signup
1. Truy cập: https://www.pinecone.io/
2. Click **"Start Free"** hoặc **"Sign Up"**
3. Chọn **"Sign up with Google"** (nhanh nhất)
4. Hoặc dùng email + password

### 1.2 Verify Email
- Check email và click link xác nhận

### 1.3 Free Plan Details
- ✅ **100,000 vectors** (đủ cho 100+ files code)
- ✅ **1 index**
- ✅ **1 pod** (serverless)
- ✅ **Không cần thẻ tín dụng**
- ✅ **Không giới hạn queries**

---

## 📌 **STEP 2: CREATE INDEX**

### 2.1 Login to Pinecone Console
- Truy cập: https://app.pinecone.io/

### 2.2 Create Index
1. Click **"Create Index"**
2. **Index name**: `sms-codebase`
3. **Dimensions**: `1536` (OpenAI text-embedding-ada-002)
4. **Metric**: `cosine`
5. **Cloud**: `AWS`
6. **Region**: `us-east-1` (free tier)
7. Click **"Create Index"**

⏱️ **Wait 2-3 minutes** cho index khởi tạo

### 2.3 Get API Key
1. Click **"API Keys"** tab (left sidebar)
2. Copy **API Key** (starts with `pc-...`)
3. Copy **Environment** (ví dụ: `us-east-1-aws`)

---

## 📌 **STEP 3: UPDATE CONFIG**

Mở `appsettings.Development.json` và update:

```json
{
  "Pinecone": {
    "ApiKey": "pc-YOUR_API_KEY_HERE",
    "Environment": "us-east-1-aws",
    "IndexName": "sms-codebase"
  }
}
```

---

## 📌 **STEP 4: INDEX CODEBASE**

Tôi sẽ tạo script Python để index toàn bộ code vào Pinecone.

**Requirements:**
```bash
pip install openai pinecone-client
```

**Run Indexing:**
```bash
python index_codebase.py
```

---

## 📊 **EXPECTED RESULTS**

### Before (Sample Docs)
- **Accuracy**: ~60%
- **Sources**: 3 hardcoded files
- **Coverage**: Controllers only

### After (Pinecone)
- **Accuracy**: ~90%
- **Sources**: 5-10 most relevant from 100+ files
- **Coverage**: Controllers, Models, Services, Angular components, all code!

---

## 🎯 **NEXT STEPS**

1. ✅ Signup Pinecone
2. ✅ Create index `sms-codebase`
3. ✅ Get API key
4. ✅ Update `appsettings.json`
5. ⏳ Run `index_codebase.py` (tôi sẽ tạo)
6. ⏳ Test RAG với real code search

---

**Sẵn sàng để tôi tạo indexing script chưa?** 🚀
