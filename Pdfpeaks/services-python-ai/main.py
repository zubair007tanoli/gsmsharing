"""
Pdfpeaks AI Microservice - FastAPI
Advanced AI-powered document processing with LangChain integration
"""

import os
import sys
import uuid
import asyncio
import tempfile
import logging
from pathlib import Path
from datetime import datetime
from typing import Optional, List, Dict, Any
from concurrent.futures import ThreadPoolExecutor

import aiofiles
from fastapi import FastAPI, UploadFile, File, Form, HTTPException, BackgroundTasks, Depends, Header
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import FileResponse, JSONResponse
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
from pydantic import BaseModel, Field
import uvicorn

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s - %(name)s - %(levelname)s - %(message)s"
)
logger = logging.getLogger(__name__)

app = FastAPI(
    title="Pdfpeaks AI Service",
    description="Advanced AI-powered document processing API",
    version="2.0.0",
    docs_url="/docs",
    redoc_url="/redoc"
)

# CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Security
security = HTTPBearer()

# Thread pool for CPU-intensive tasks
executor = ThreadPoolExecutor(max_workers=4)

# Configuration
AI_API_KEY = os.getenv("AI_API_KEY", "default-key")
QDRANT_URL = os.getenv("QDRANT_URL", "localhost")
QDRANT_API_KEY = os.getenv("QDRANT_API_KEY")


# ============ Pydantic Models ============

class DocumentProcessRequest(BaseModel):
    """Request model for document processing"""
    process_type: str = Field(..., description="Type of processing: extract, summarize, qa, classify")
    language: str = Field(default="auto", description="Language of document")
    options: Optional[Dict[str, Any]] = Field(default=None, description="Additional options")


class ProcessResponse(BaseModel):
    """Response model for document processing"""
    task_id: str
    status: str
    message: str
    result: Optional[Dict[str, Any]] = None
    processing_time_ms: float


class QnARequest(BaseModel):
    """Request model for question answering"""
    question: str
    max_sources: int = Field(default=5, ge=1, le=20)


class QnAResponse(BaseModel):
    """Response model for Q&A"""
    answer: str
    sources: List[Dict[str, Any]]
    confidence: float


class SummaryRequest(BaseModel):
    """Request model for summarization"""
    max_length: int = Field(default=200, ge=50, ge=1000)
    style: str = Field(default="concise", pattern="^(concise|detailed|bullet)$")


class HealthResponse(BaseModel):
    """Health check response"""
    status: str
    version: str
    components: Dict[str, str]


# ============ Authentication ============

async def verify_api_key(credentials: HTTPAuthorizationCredentials = Depends(security)):
    """Verify API key for protected endpoints"""
    if credentials.credentials != AI_API_KEY:
        raise HTTPException(status_code=401, detail="Invalid API key")
    return credentials.credentials


# ============ Helper Functions ============

async def save_uploaded_file(upload_file: UploadFile) -> str:
    """Save uploaded file to temp storage"""
    temp_dir = tempfile.gettempdir()
    file_id = str(uuid.uuid4())
    file_path = os.path.join(temp_dir, f"{file_id}_{upload_file.filename}")
    
    async with aiofiles.open(file_path, 'wb') as f:
        content = await upload_file.read()
        await f.write(content)
    
    logger.info(f"Saved uploaded file: {file_path}")
    return file_path


def cleanup_file(file_path: str):
    """Clean up temporary file"""
    try:
        if os.path.exists(file_path):
            os.remove(file_path)
            logger.info(f"Cleaned up file: {file_path}")
    except Exception as e:
        logger.error(f"Error cleaning up file: {e}")


# ============ AI Processing Functions ============

async def extract_text_from_pdf(pdf_path: str) -> Dict[str, Any]:
    """Extract text from PDF using pdfplumber"""
    try:
        import pdfplumber
        import re
        
        text_content = {}
        with pdfplumber.open(pdf_path) as pdf:
            for i, page in enumerate(pdf.pages):
                page_text = page.extract_text() or ""
                # Clean and normalize text
                page_text = re.sub(r'\s+', ' ', page_text).strip()
                text_content[f"page_{i+1}"] = page_text
        
        full_text = " ".join(text_content.values())
        
        return {
            "success": True,
            "total_pages": len(text_content),
            "text": full_text,
            "pages": text_content
        }
    except Exception as e:
        logger.error(f"PDF extraction error: {e}")
        return {"success": False, "error": str(e)}


async def summarize_document(text: str, max_length: int, style: str) -> Dict[str, Any]:
    """Summarize document using LangChain"""
    try:
        from langchain import LangChain
        from langchain.chains.summarize import load_summarize_chain
        from langchain_openai import ChatOpenAI
        from langchain.prompts import PromptTemplate
        
        # This would use actual OpenAI in production
        # For demo, use simple extraction
        
        sentences = text.split('.')
        words = text.split()
        
        if style == "concise":
            target_words = min(max_length, len(words) // 3)
        elif style == "detailed":
            target_words = min(max_length, len(words) // 2)
        else:  # bullet
            target_words = min(max_length, len(words) // 2)
        
        # Simple extractive summarization
        summary_sentences = sentences[:min(len(sentences) // 2, target_words // 10)]
        summary = '. '.join(summary_sentences)
        
        if style == "bullet":
            bullet_points = [f"- {s.strip()}" for s in summary_sentences if len(s.strip()) > 20]
            summary = '\n'.join(bullet_points)
        
        return {
            "success": True,
            "summary": summary,
            "original_length": len(words),
            "summary_length": len(summary.split()),
            "compression_ratio": len(summary) / max(len(text), 1)
        }
    except Exception as e:
        logger.error(f"Summarization error: {e}")
        return {"success": False, "error": str(e)}


async def answer_question(context: str, question: str) -> Dict[str, Any]:
    """Answer question based on document context"""
    try:
        # Simple extractive QA (would use LangChain RAG in production)
        question_words = question.lower().split()
        context_lower = context.lower()
        
        # Find relevant sentences
        sentences = context.split('.')
        relevant_sentences = []
        
        for sentence in sentences:
            sentence_lower = sentence.lower()
            matches = sum(1 for word in question_words if word in sentence_lower)
            if matches > 0:
                relevant_sentences.append((sentence, matches))
        
        # Sort by relevance
        relevant_sentences.sort(key=lambda x: x[1], reverse=True)
        
        # Get top sources
        sources = []
        for i, (sentence, score) in enumerate(relevant_sentences[:5]):
            sources.append({
                "text": sentence.strip(),
                "relevance_score": score / max(len(question_words), 1),
                "source_id": f"chunk_{i}"
            })
        
        # Generate answer
        answer = " ".join([s[0] for s in relevant_sentences[:3]])
        
        return {
            "success": True,
            "answer": answer if answer else "I couldn't find a specific answer to your question.",
            "sources": sources,
            "confidence": min(0.95, len(relevant_sentences) * 0.15 + 0.3)
        }
    except Exception as e:
        logger.error(f"QA error: {e}")
        return {"success": False, "error": str(e)}


async def classify_document(text: str) -> Dict[str, Any]:
    """Classify document into categories"""
    try:
        # Simple keyword-based classification
        categories = {
            "Invoice": ["invoice", "payment", "amount", "due", "total", "bill"],
            "Contract": ["contract", "agreement", "terms", "party", "signature", "legal"],
            "Report": ["report", "analysis", "findings", "conclusion", "recommendation"],
            "Form": ["form", "application", "register", "填写", "please"],
            "Manual": ["manual", "guide", "instructions", "step", "procedure"],
            "Letter": ["dear", "sincerely", "regards", "letter"]
        }
        
        text_lower = text.lower()
        scores = {}
        
        for category, keywords in categories.items():
            score = sum(1 for kw in keywords if kw in text_lower)
            scores[category] = score
        
        sorted_scores = sorted(scores.items(), key=lambda x: x[1], reverse=True)
        top_category = sorted_scores[0] if sorted_scores[0][1] > 0 else ("Other", 0)
        
        return {
            "success": True,
            "category": top_category[0],
            "confidence": min(0.95, top_category[1] / 5.0 + 0.3),
            "all_scores": dict(sorted_scores[:5])
        }
    except Exception as e:
        logger.error(f"Classification error: {e}")
        return {"success": False, "error": str(e)}


async def extract_entities(text: str) -> Dict[str, Any]:
    """Extract named entities from text"""
    try:
        # Simple entity extraction using patterns
        import re
        
        # Email patterns
        emails = re.findall(r'\b[\w.-]+@[\w.-]+\.\w+\b', text)
        
        # Phone patterns
        phones = re.findall(r'\b[\d\-\(\)\+\s]{10,}\b', text)
        
        # Date patterns
        dates = re.findall(r'\b\d{1,2}[/-]\d{1,2}[/-]\d{2,4}\b', text)
        dates += re.findall(r'\b(?:Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[a-z]* \d{1,2},? \d{4}\b', text, re.IGNORECASE)
        
        # Currency patterns
        currencies = re.findall(r'\$[\d,]+\.?\d*|\bUSD\b|\bEUR\b|\bGBP\b', text, re.IGNORECASE)
        
        return {
            "success": True,
            "entities": {
                "emails": list(set(emails)),
                "phones": list(set(phones)),
                "dates": list(set(dates)),
                "currencies": list(set(currencies))
            }
        }
    except Exception as e:
        logger.error(f"Entity extraction error: {e}")
        return {"success": False, "error": str(e)}


# ============ API Endpoints ============

@app.get("/", tags=["Root"])
async def root():
    """Root endpoint"""
    return {
        "service": "Pdfpeaks AI Service",
        "version": "2.0.0",
        "status": "running"
    }


@app.get("/health", response_model=HealthResponse, tags=["Health"])
async def health_check():
    """Health check endpoint"""
    return HealthResponse(
        status="healthy",
        version="2.0.0",
        components={
            "api": "ok",
            "storage": "ok"
        }
    )


@app.post("/api/v1/process", response_model=ProcessResponse, tags=["Processing"])
async def process_document(
    file: UploadFile = File(...),
    process_type: str = Form(...),
    api_key: str = Depends(verify_api_key)
):
    """Process document with specified type"""
    start_time = datetime.utcnow()
    task_id = str(uuid.uuid4())
    
    try:
        # Save uploaded file
        file_path = await save_uploaded_file(file)
        
        # Extract text
        extraction = await extract_text_from_pdf(file_path)
        if not extraction.get("success"):
            raise HTTPException(status_code=400, detail="Failed to extract text from PDF")
        
        full_text = extraction["text"]
        
        # Process based on type
        result = {}
        if process_type == "summarize":
            result = await summarize_document(full_text, 200, "concise")
        elif process_type == "classify":
            result = await classify_document(full_text)
        elif process_type == "extract_entities":
            result = await extract_entities(full_text)
        elif process_type == "full_analysis":
            result = {
                "text_length": len(full_text),
                "pages": extraction["total_pages"],
                "summary": await summarize_document(full_text, 200, "concise"),
                "classification": await classify_document(full_text),
                "entities": await extract_entities(full_text)
            }
        else:
            raise HTTPException(status_code=400, detail=f"Unknown process type: {process_type}")
        
        # Cleanup
        cleanup_file(file_path)
        
        processing_time = (datetime.utcnow() - start_time).total_seconds() * 1000
        
        return ProcessResponse(
            task_id=task_id,
            status="completed",
            message="Document processed successfully",
            result=result,
            processing_time_ms=processing_time
        )
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Processing error: {e}")
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/api/v1/qa", response_model=QnAResponse, tags=["Q&A"])
async def question_answer(
    request: QnARequest,
    file: UploadFile = File(...),
    api_key: str = Depends(verify_api_key)
):
    """Answer questions about uploaded document"""
    try:
        # Save and process file
        file_path = await save_uploaded_file(file)
        
        # Extract text
        extraction = await extract_text_from_pdf(file_path)
        if not extraction.get("success"):
            raise HTTPException(status_code=400, detail="Failed to extract text")
        
        # Get answer
        result = await answer_question(extraction["text"], request.question)
        
        # Cleanup
        cleanup_file(file_path)
        
        if not result.get("success"):
            raise HTTPException(status_code=500, detail=result.get("error"))
        
        return QnAResponse(
            answer=result["answer"],
            sources=result["sources"],
            confidence=result["confidence"]
        )
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Q&A error: {e}")
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/api/v1/summarize", tags=["Summarization"])
async def summarize(
    file: UploadFile = File(...),
    max_length: int = Form(default=200, ge=50, le=1000),
    style: str = Form(default="concise", pattern="^(concise|detailed|bullet)$"),
    api_key: str = Depends(verify_api_key)
):
    """Summarize uploaded document"""
    try:
        file_path = await save_uploaded_file(file)
        
        extraction = await extract_text_from_pdf(file_path)
        if not extraction.get("success"):
            raise HTTPException(status_code=400, detail="Failed to extract text")
        
        result = await summarize_document(extraction["text"], max_length, style)
        
        cleanup_file(file_path)
        
        if not result.get("success"):
            raise HTTPException(status_code=500, detail=result.get("error"))
        
        return {
            "summary": result["summary"],
            "original_length": result["original_length"],
            "summary_length": result["summary_length"],
            "compression_ratio": result["compression_ratio"]
        }
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Summarize error: {e}")
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/api/v1/classify", tags=["Classification"])
async def classify(
    file: UploadFile = File(...),
    api_key: str = Depends(verify_api_key)
):
    """Classify uploaded document"""
    try:
        file_path = await save_uploaded_file(file)
        
        extraction = await extract_text_from_pdf(file_path)
        if not extraction.get("success"):
            raise HTTPException(status_code=400, detail="Failed to extract text")
        
        result = await classify_document(extraction["text"])
        
        cleanup_file(file_path)
        
        return result
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Classify error: {e}")
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/api/v1/extract-text", tags=["Extraction"])
async def extract_text(
    file: UploadFile = File(...),
    api_key: str = Depends(verify_api_key)
):
    """Extract raw text from uploaded document"""
    try:
        file_path = await save_uploaded_file(file)
        
        result = await extract_text_from_pdf(file_path)
        
        cleanup_file(file_path)
        
        if not result.get("success"):
            raise HTTPException(status_code=400, detail=result.get("error"))
        
        return result
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Extract text error: {e}")
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/api/v1/extract-entities", tags=["Extraction"])
async def extract_named_entities(
    file: UploadFile = File(...),
    api_key: str = Depends(verify_api_key)
):
    """Extract named entities from uploaded document"""
    try:
        file_path = await save_uploaded_file(file)
        
        extraction = await extract_text_from_pdf(file_path)
        if not extraction.get("success"):
            raise HTTPException(status_code=400, detail="Failed to extract text")
        
        result = await extract_entities(extraction["text"])
        
        cleanup_file(file_path)
        
        return result
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Entity extraction error: {e}")
        raise HTTPException(status_code=500, detail=str(e))


# ============ Error Handlers ============

@app.exception_handler(Exception)
async def global_exception_handler(request, exc):
    """Global exception handler"""
    logger.error(f"Unhandled exception: {exc}")
    return JSONResponse(
        status_code=500,
        content={"detail": "Internal server error"}
    )


# ============ Main Entry Point ============

if __name__ == "__main__":
    port = int(os.getenv("PORT", 8001))
    host = os.getenv("HOST", "0.0.0.0")
    
    uvicorn.run(
        "main:app",
        host=host,
        port=port,
        reload=False,
        log_level="info"
    )
