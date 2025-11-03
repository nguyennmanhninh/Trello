import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, BehaviorSubject, throwError } from 'rxjs';
import { map, catchError, retry, delay } from 'rxjs/operators';

export interface ChatMessage {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  timestamp: Date;
  sources?: CodeSource[];
  followUpQuestions?: string[];
  isMarkdown?: boolean;
}

export interface RateLimitInfo {
  remaining: number;
  total: number;
  resetTime: Date;
}

export interface CodeSource {
  fileName: string;
  filePath: string;
  codeSnippet: string;
  score: number;
  language: string;
}

@Injectable({
  providedIn: 'root'
})
export class AiRagChatService {
  private readonly apiUrl = '/api/chat';
  private messagesSubject = new BehaviorSubject<ChatMessage[]>([]);
  public messages$ = this.messagesSubject.asObservable();

  private loadingSubject = new BehaviorSubject<boolean>(false);
  public loading$ = this.loadingSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadHistory();
  }

  askQuestion(question: string): Observable<ChatMessage> {
    const q = question.trim();
    if (q.length < 3) {
      return throwError(() => new Error('Câu hỏi phải có ít nhất 3 ký tự'));
    }

    this.loadingSubject.next(true);

    const userMsg: ChatMessage = {
      id: this.genId(),
      role: 'user',
      content: q,
      timestamp: new Date()
    };
    this.addMessage(userMsg);

    return this.http.post<any>(`${this.apiUrl}/ask`, { Question: q }).pipe(
      retry({ count: 2, delay: 1000 }),
      map(res => {
        this.loadingSubject.next(false);
        
        const data = res.Data || res.data;
        if (!data) throw new Error('No response data');

        const aiMsg: ChatMessage = {
          id: this.genId(),
          role: 'assistant',
          content: data.Answer || data.answer || '',
          timestamp: new Date(),
          sources: (data.Sources || data.sources || []).map((s: any) => ({
            fileName: s.FileName || s.fileName || '',
            filePath: s.FilePath || s.filePath || '',
            codeSnippet: s.CodeSnippet || s.codeSnippet || '',
            score: s.Score || s.score || 0,
            language: this.detectLang(s.FileName || s.fileName || '')
          })),
          followUpQuestions: data.FollowUpQuestions || data.followUpQuestions || []
        };

        this.addMessage(aiMsg);
        return aiMsg;
      }),
      catchError((err: HttpErrorResponse) => {
        this.loadingSubject.next(false);
        let msg = 'Lỗi kết nối AI';
        
        if (err.status === 400) {
          msg = '❌ Câu hỏi không hợp lệ (3-1000 ký tự)';
        } else if (err.status === 429) {
          msg = '⏱️ Gemini API rate limit! Đợi 1 phút rồi thử lại. Free tier: 15 requests/phút';
        } else if (err.status === 500) {
          msg = '🔥 Gemini API đang bị rate limit (429)! Đợi 1-2 phút rồi thử lại nhé. Free tier giới hạn 15 requests/phút';
        } else if (err.status === 0) {
          msg = '🌐 Không kết nối được backend. Kiểm tra server đã chạy chưa?';
        }
        
        // Add error message to chat
        const errMsg: ChatMessage = {
          id: this.genId(),
          role: 'assistant',
          content: msg,
          timestamp: new Date(),
          sources: [],
          followUpQuestions: []
        };
        this.addMessage(errMsg);
        
        return throwError(() => new Error(msg));
      })
    );
  }

  clearChat(): void {
    this.messagesSubject.next([]);
    localStorage.removeItem('ai-rag-chat');
  }

  private addMessage(msg: ChatMessage): void {
    const msgs = [...this.messagesSubject.value, msg];
    this.messagesSubject.next(msgs);
    this.saveHistory(msgs);
  }

  private saveHistory(msgs: ChatMessage[]): void {
    try {
      localStorage.setItem('ai-rag-chat', JSON.stringify(msgs.slice(-50)));
    } catch (e) {}
  }

  private loadHistory(): void {
    try {
      const data = localStorage.getItem('ai-rag-chat');
      if (data) {
        const msgs = JSON.parse(data);
        msgs.forEach((m: any) => m.timestamp = new Date(m.timestamp));
        this.messagesSubject.next(msgs);
      }
    } catch (e) {}
  }

  private detectLang(file: string): string {
    const ext = file.split('.').pop()?.toLowerCase();
    const map: any = {
      'ts': 'typescript', 'js': 'javascript', 'cs': 'csharp',
      'html': 'html', 'css': 'css', 'json': 'json', 'sql': 'sql'
    };
    return map[ext || ''] || 'plaintext';
  }

  private genId(): string {
    return `msg_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  get messages(): ChatMessage[] {
    return this.messagesSubject.value;
  }

  get isLoading(): boolean {
    return this.loadingSubject.value;
  }
}
