import { Component, OnInit, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { AiRagChatService, ChatMessage } from '../../services/ai-rag-chat.service';
import { marked } from 'marked';
import hljs from 'highlight.js';

@Component({
  selector: 'app-ai-rag-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ai-rag-chat.component.html',
  styleUrl: './ai-rag-chat.component.scss'
})
export class AiRagChatComponent implements OnInit, AfterViewChecked {
  @ViewChild('messagesContainer') messagesContainer!: ElementRef;
  
  messages: ChatMessage[] = [];
  currentQuestion = '';
  loading = false;
  isMinimized = false;
  isVisible = true;
  showSamples = true;
  selectedMsgIdx: number | null = null;
  
  // Backend data context
  private systemContext = {
    totalStudents: 0,
    totalTeachers: 0,
    totalClasses: 0,
    totalCourses: 0,
    recentActivities: [] as any[]
  };

  sampleQuestions = [
    'Có bao nhiêu sinh viên trong hệ thống?',
    'Thống kê số lượng giảng viên và lớp học?',
    'Các hoạt động gần đây trong hệ thống?',
    'Phân bổ sinh viên theo khoa như thế nào?'
  ];

  constructor(
    public aiService: AiRagChatService,
    private http: HttpClient
  ) {
    // Setup marked for markdown rendering
    marked.setOptions({
      breaks: true,
      gfm: true
    });
  }

  ngOnInit(): void {
    this.loadSystemContext();
    
    this.aiService.messages$.subscribe(msgs => {
      this.messages = msgs;
    });

    this.aiService.loading$.subscribe(loading => {
      this.loading = loading;
    });
  }

  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }

  private loadSystemContext(): void {
    // Load dashboard statistics from backend
    this.http.get<any>('/api/dashboard/stats').subscribe({
      next: (data) => {
        this.systemContext = {
          totalStudents: data.totalStudents || 0,
          totalTeachers: data.totalTeachers || 0,
          totalClasses: data.totalClasses || 0,
          totalCourses: data.totalCourses || 0,
          recentActivities: []
        };
      },
      error: (err) => {
        console.warn('Could not load system context:', err);
      }
    });
  }

  sendQuestion(): void {
    const q = this.currentQuestion.trim();
    if (!q || this.loading || q.length < 3) {
      if (q.length > 0 && q.length < 3) {
        alert('❌ Câu hỏi phải có ít nhất 3 ký tự');
      }
      return;
    }

    // Add system context to question
    let enrichedQuestion = q;
    if (q.toLowerCase().includes('sinh viên') || q.toLowerCase().includes('student')) {
      enrichedQuestion += `\n\nContext: Hệ thống hiện có ${this.systemContext.totalStudents} sinh viên, ${this.systemContext.totalTeachers} giảng viên, ${this.systemContext.totalClasses} lớp học, và ${this.systemContext.totalCourses} môn học.`;
    }

    this.currentQuestion = '';
    this.showSamples = false;

    this.aiService.askQuestion(enrichedQuestion).subscribe({
      next: () => {
        // Message already added to stream by service
        // No need to do anything here - messages$ subscription will handle it
        this.showSamples = true;
      },
      error: (err) => {
        // Error message already added to stream by service
        this.showSamples = true;
      }
    });
  }

  askSample(q: string): void {
    this.currentQuestion = q;
    this.sendQuestion();
  }

  toggleSources(idx: number): void {
    this.selectedMsgIdx = this.selectedMsgIdx === idx ? null : idx;
  }

  copyCode(code: string): void {
    navigator.clipboard.writeText(code).then(() => {
      alert('✅ Đã copy code!');
    });
  }

  clearChat(): void {
    if (confirm('Xóa toàn bộ lịch sử chat?')) {
      this.aiService.clearChat();
      this.showSamples = true;
    }
  }

  toggleMinimize(): void {
    this.isMinimized = !this.isMinimized;
  }

  closeChat(): void {
    this.isVisible = false;
  }

  openChat(): void {
    this.isVisible = true;
    this.isMinimized = false;
  }

  private scrollToBottom(): void {
    try {
      if (this.messagesContainer) {
        this.messagesContainer.nativeElement.scrollTop = 
          this.messagesContainer.nativeElement.scrollHeight;
      }
    } catch (e) {}
  }

  onKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendQuestion();
    }
  }

  formatTime(date: Date): string {
    return new Date(date).toLocaleTimeString('vi-VN', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getFileIcon(file: string): string {
    if (file.endsWith('.cs')) return '📄';
    if (file.endsWith('.ts')) return '📘';
    if (file.endsWith('.html')) return '🌐';
    return '📁';
  }

  // Render markdown to HTML
  renderMarkdown(text: string): string {
    try {
      return marked.parse(text) as string;
    } catch (e) {
      return text;
    }
  }

  // Explain code snippet
  explainCode(code: string): void {
    this.currentQuestion = `Giải thích đoạn code này:\n\`\`\`\n${code}\n\`\`\``;
    this.sendQuestion();
  }

  // Get related questions for code
  getCodeRelatedQuestions(code: string): string[] {
    return [
      '💡 Giải thích code này',
      '🔧 Có cách viết tốt hơn không?',
      '🐛 Có bug tiềm ẩn nào không?',
      '⚡ Làm sao optimize performance?',
      '📝 Generate unit test cho code này'
    ];
  }

  // Ask related question about code
  askCodeQuestion(code: string, questionType: string): void {
    const questions: any = {
      '💡 Giải thích code này': `Giải thích chi tiết đoạn code này:\n\`\`\`\n${code}\n\`\`\``,
      '🔧 Có cách viết tốt hơn không?': `Refactor đoạn code này để tốt hơn:\n\`\`\`\n${code}\n\`\`\``,
      '🐛 Có bug tiềm ẩn nào không?': `Tìm các bug hoặc vấn đề trong code:\n\`\`\`\n${code}\n\`\`\``,
      '⚡ Làm sao optimize performance?': `Làm sao optimize performance của code này:\n\`\`\`\n${code}\n\`\`\``,
      '📝 Generate unit test cho code này': `Generate unit test cho đoạn code:\n\`\`\`\n${code}\n\`\`\``
    };
    
    this.currentQuestion = questions[questionType] || questionType;
    this.sendQuestion();
  }
}

