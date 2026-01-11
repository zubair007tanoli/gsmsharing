#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Ultra-simple test server for MCP - No dependencies required!
Uses only Python standard library
"""

import sys
import io

# Fix Windows console encoding issues
if sys.platform == 'win32':
    # Set UTF-8 encoding for stdout/stderr on Windows
    if hasattr(sys.stdout, 'reconfigure'):
        sys.stdout.reconfigure(encoding='utf-8', errors='replace')
        sys.stderr.reconfigure(encoding='utf-8', errors='replace')
    else:
        # Fallback for older Python versions
        sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
        sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')

from http.server import HTTPServer, BaseHTTPRequestHandler
import json
from datetime import datetime, timezone

class MCPHandler(BaseHTTPRequestHandler):
    def do_GET(self):
        if self.path == '/health':
            self.send_response(200)
            self.send_header('Content-Type', 'application/json')
            self.send_header('Access-Control-Allow-Origin', '*')
            self.end_headers()
            
            response = {
                "status": "healthy",
                "timestamp": datetime.now(timezone.utc).isoformat(),
                "server": "SEO Automation (Test Mode)",
                "ai_available": False,
                "model": "none (test server)"
            }
            
            self.wfile.write(json.dumps(response).encode())
        else:
            self.send_response(404)
            self.end_headers()
    
    def do_POST(self):
        if self.path == '/mcp':
            content_length = int(self.headers.get('Content-Length', 0))
            body = self.rfile.read(content_length)
            
            try:
                request_data = json.loads(body.decode())
                method = request_data.get('method')
                request_id = request_data.get('id')
                
                # Simple mock responses
                result = {
                    "analyze_keywords": ["test", "keyword", "seo"],
                    "optimize_content": {"optimized": True, "score": 75},
                    "get_competitor_insights": {"score": 70, "suggestions": ["Add more keywords"]},
                    "generate_meta_description": "Test meta description",
                    "optimize_title": "Optimized Title"
                }.get(method, {"message": "Method not implemented"})
                
                response = {
                    "jsonrpc": "2.0",
                    "result": result,
                    "id": request_id
                }
                
                self.send_response(200)
                self.send_header('Content-Type', 'application/json')
                self.send_header('Access-Control-Allow-Origin', '*')
                self.end_headers()
                self.wfile.write(json.dumps(response).encode())
            except Exception as e:
                self.send_response(500)
                self.end_headers()
                error_response = {"error": str(e)}
                self.wfile.write(json.dumps(error_response).encode())
        else:
            self.send_response(404)
            self.end_headers()
    
    def log_message(self, format, *args):
        # Custom logging
        print(f"[{datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] {format % args}")

if __name__ == '__main__':
    import sys
    PORT = 5001
    
    # Allow port to be overridden via command line argument
    if len(sys.argv) > 1:
        try:
            PORT = int(sys.argv[1])
        except ValueError:
            print(f"Invalid port argument: {sys.argv[1]}, using default 5001")
    
    server = HTTPServer(('0.0.0.0', PORT), MCPHandler)
    
    # Safe print function that handles encoding errors
    def safe_print(text):
        try:
            print(text, flush=True)
        except UnicodeEncodeError:
            # Fallback to ASCII if encoding fails
            print(text.encode('ascii', errors='replace').decode('ascii'), flush=True)
    
    safe_print("=" * 70)
    safe_print("[MCP] SEO Automation Test Server")
    safe_print("=" * 70)
    safe_print(f"[OK] Server running on http://localhost:{PORT}")
    safe_print(f"[OK] Health check: http://localhost:{PORT}/health")
    safe_print("[OK] No dependencies required - uses Python standard library only!")
    safe_print("=" * 70)
    safe_print("Press Ctrl+C to stop")
    safe_print("")
    
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        safe_print("\n\n[STOP] Server stopped")
        server.shutdown()

