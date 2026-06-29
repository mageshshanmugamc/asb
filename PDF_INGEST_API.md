# PDF Document Ingestion API

## Overview
The PDF Document Ingestion API allows you to upload PDF files directly to the knowledge base. The API automatically extracts text from the PDF and ingests it into Qdrant for RAG (Retrieval-Augmented Generation) retrieval.

## New Endpoint

### POST `/api/v1/agent/ingest/pdf`

**Content-Type:** `multipart/form-data`

**Description:** Upload a PDF document to the knowledge base. The text is extracted from the PDF and chunked for embedding and storage in Qdrant.

#### Request Parameters

| Parameter | Type | Required | Description | Default |
|-----------|------|----------|-------------|---------|
| `pdfFile` | file | ✓ Yes | The PDF file to upload (multipart form data) | - |
| `source` | string | No | Custom document identifier/name. If omitted, uses the filename without extension | Filename without extension |
| `chunkSize` | integer | No | Size of text chunks for RAG processing (characters) | 500 |
| `chunkOverlap` | integer | No | Overlap between chunks for context preservation (characters) | 50 |

#### Constraints
- **File Type:** PDF only (checked via MIME type: `application/pdf`)
- **Max File Size:** 50 MB
- **Chunk Size:** 1-10000 characters
- **Chunk Overlap:** 0 or positive, must be less than chunk size

#### Response (200 OK)

```json
{
  "success": true,
  "message": "PDF document successfully ingested into knowledge base.",
  "fileName": "my-document.pdf",
  "source": "my-document",
  "chunksCreated": 45,
  "textLength": 24567,
  "fileSize": 1048576
}
```

#### Response Fields
| Field | Type | Description |
|-------|------|-------------|
| `success` | boolean | Whether the PDF was successfully processed |
| `message` | string | Status message |
| `fileName` | string | Original PDF filename |
| `source` | string | Document identifier used in knowledge base |
| `chunksCreated` | integer | Number of text chunks created |
| `textLength` | integer | Total extracted text length (characters) |
| `fileSize` | long | Uploaded file size (bytes) |

#### Error Responses

**400 Bad Request**
```json
{
  "error": "PDF file is required."
}
```

```json
{
  "error": "No text content could be extracted from the PDF file."
}
```

**500 Internal Server Error**
```json
{
  "error": "An error occurred while processing the PDF file.",
  "details": "Failed to process PDF file. Please ensure it is a valid PDF document."
}
```

## Existing Text Ingest Endpoint

For reference, the original text-based endpoint is still available:

### POST `/api/v1/agent/ingest`

**Content-Type:** `application/json`

**Request Body:**
```json
{
  "content": "Your text content here...",
  "source": "document-name",
  "chunkSize": 500,
  "chunkOverlap": 50
}
```

## Usage Examples

### Using cURL
```bash
# Basic file upload (uses filename as source)
curl -X POST "http://localhost:5000/api/v1/agent/ingest/pdf" \
  -F "pdfFile=@my-document.pdf"

# Upload with custom source name and chunk parameters
curl -X POST "http://localhost:5000/api/v1/agent/ingest/pdf" \
  -F "pdfFile=@my-document.pdf" \
  -F "source=my-knowledge-base-doc" \
  -F "chunkSize=750" \
  -F "chunkOverlap=100"
```

### Using PowerShell
```powershell
# Basic file upload
$params = @{
    Uri = "http://localhost:5000/api/v1/agent/ingest/pdf"
    Method = "POST"
    Form = @{
        pdfFile = Get-Item -Path "C:\path\to\my-document.pdf"
    }
}
Invoke-RestMethod @params

# Upload with custom parameters
$params = @{
    Uri = "http://localhost:5000/api/v1/agent/ingest/pdf"
    Method = "POST"
    Form = @{
        pdfFile = Get-Item -Path "C:\path\to\my-document.pdf"
        source = "my-knowledge-base-doc"
        chunkSize = 750
        chunkOverlap = 100
    }
}
Invoke-RestMethod @params
```

### Using JavaScript/Fetch
```javascript
const formData = new FormData();
const fileInput = document.getElementById('pdfFile');

formData.append('pdfFile', fileInput.files[0]);
formData.append('source', 'my-document');
formData.append('chunkSize', '500');
formData.append('chunkOverlap', '50');

const response = await fetch('http://localhost:5000/api/v1/agent/ingest/pdf', {
  method: 'POST',
  body: formData
});

const result = await response.json();
console.log(result);
```

### Using Swagger UI
1. Navigate to `http://localhost:5000/swagger`
2. Find the **POST /api/v1/agent/ingest/pdf** endpoint
3. Click "Try it out"
4. Click "Choose File" and select your PDF
5. Enter optional parameters (source, chunkSize, chunkOverlap)
6. Click "Execute"

## Implementation Details

### Architecture
- **File Upload Handler:** Accepts multipart/form-data with PDF file
- **PDF Text Extractor:** Uses UglyToad.PdfPig library to extract text
- **Text Chunking:** Splits extracted text into overlapping chunks
- **Embedding Generation:** Uses Ollama to generate embeddings
- **Vector Storage:** Stores embeddings in Qdrant
- **Validation:** FluentValidation for input validation

### Processing Flow
1. Receive multipart form data with PDF file
2. Validate file type (PDF) and size (max 50 MB)
3. Extract text content from all pages
4. Use custom source name or filename as document identifier
5. Split text into chunks with specified size and overlap
6. Generate embeddings via Ollama
7. Store in Qdrant vector database
8. Return summary with chunk count and text statistics

### Dependencies
- **UglyToad.PdfPig (1.7.0-custom-5):** PDF text extraction
- **Ollama:** Embedding generation (via existing RagService)
- **Qdrant:** Vector database (via existing RagService)
- **FluentValidation:** Input validation
- **Serilog:** Logging

## Files Modified/Created

### New Files
- `ASB.Admin/v1/Requests/IngestPdfRequest.cs` - PDF upload request model
- `ASB.Admin/v1/Validators/IngestPdfRequestValidator.cs` - Input validation
- `ASB.Admin/v1/Response/PdfIngestResponse.cs` - Response model
- `ASB.Admin/v1/Services/PdfExtractionService.cs` - PDF text extraction service

### Modified Files
- `ASB.Admin/v1/Controllers/AgentController.cs` - Added new endpoint
- `ASB.Admin/Program.cs` - Registered PdfExtractionService in DI
- `ASB.Admin/ASB.Admin.csproj` - Added UglyToad.PdfPig package

## Testing Checklist

- [ ] Upload a simple PDF file
- [ ] Verify chunks are created correctly
- [ ] Test with custom source name
- [ ] Test with different chunk sizes
- [ ] Test with oversized file (> 50 MB)
- [ ] Test with non-PDF file
- [ ] Test with empty PDF
- [ ] Verify in Swagger UI
- [ ] Search knowledge base for ingested content
- [ ] Test chat endpoint with RAG using ingested PDF

## Future Enhancements

- Add support for other document formats (DOCX, TXT, etc.)
- Batch file upload (multiple PDFs)
- Progress tracking for large files
- OCR support for scanned PDFs
- Document metadata extraction
- Delete/update ingested documents
- Document version control
