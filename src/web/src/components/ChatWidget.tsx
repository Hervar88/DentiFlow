import { useState, useRef, useEffect, useCallback } from 'react';
import { useQuery } from '@tanstack/react-query';
import { sendChatMessage, isChatbotConfigured, type ChatMessagePayload } from '../api';
import { MessageCircle, X, Send, Bot, User, Loader2 } from 'lucide-react';

interface ChatWidgetProps {
  slug: string;
  clinicaNombre: string;
}

interface DisplayMessage {
  role: 'user' | 'assistant';
  content: string;
}

export default function ChatWidget({ slug, clinicaNombre }: ChatWidgetProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [messages, setMessages] = useState<DisplayMessage[]>([]);
  const [input, setInput] = useState('');
  const [isSending, setIsSending] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  // Check if chatbot is configured
  const { data: configData } = useQuery({
    queryKey: ['chatbot-configured'],
    queryFn: isChatbotConfigured,
    staleTime: 60_000,
  });

  const isConfigured = configData?.configured ?? false;

  // Auto-scroll to latest message
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  // Focus input when chat opens
  useEffect(() => {
    if (isOpen) inputRef.current?.focus();
  }, [isOpen]);

  // Add welcome message when chat first opens
  const handleOpen = useCallback(() => {
    setIsOpen(true);
    if (messages.length === 0) {
      setMessages([
        {
          role: 'assistant',
          content: `¡Hola! 👋 Soy Ana, la recepcionista virtual de **${clinicaNombre}**. ¿En qué puedo ayudarte hoy?`,
        },
      ]);
    }
  }, [clinicaNombre, messages.length]);

  const handleSend = async () => {
    const text = input.trim();
    if (!text || isSending) return;

    const userMsg: DisplayMessage = { role: 'user', content: text };
    const updated = [...messages, userMsg];
    setMessages(updated);
    setInput('');
    setIsSending(true);

    try {
      // Send only user/assistant messages (skip the welcome message if it was injected client-side)
      const apiMessages: ChatMessagePayload[] = updated
        .map((m) => ({ role: m.role, content: m.content }));

      const { response } = await sendChatMessage(slug, apiMessages);
      setMessages((prev) => [...prev, { role: 'assistant', content: response }]);
    } catch {
      setMessages((prev) => [
        ...prev,
        { role: 'assistant', content: 'Lo siento, tuve un problema al procesar tu mensaje. Intenta de nuevo. 😅' },
      ]);
    } finally {
      setIsSending(false);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  // Don't render if not configured
  if (!isConfigured) return null;

  return (
    <>
      {/* ═══ Floating Bubble ═══ */}
      {!isOpen && (
        <button
          onClick={handleOpen}
          className="fixed bottom-6 right-6 z-50 w-14 h-14 rounded-full bg-gradient-to-r from-sky-600 to-cyan-500 text-white shadow-xl shadow-sky-500/30 hover:shadow-sky-500/50 hover:scale-110 transition-all duration-300 flex items-center justify-center group"
          aria-label="Abrir chat"
        >
          <MessageCircle size={24} className="group-hover:scale-110 transition-transform" />
          {/* Ping animation */}
          <span className="absolute -top-1 -right-1 w-4 h-4 bg-green-400 rounded-full border-2 border-white animate-pulse" />
        </button>
      )}

      {/* ═══ Chat Panel ═══ */}
      {isOpen && (
        <div className="fixed bottom-6 right-6 z-50 w-[380px] max-w-[calc(100vw-2rem)] h-[520px] max-h-[calc(100vh-3rem)] bg-white rounded-2xl shadow-2xl border border-gray-200 flex flex-col overflow-hidden animate-in fade-in slide-in-from-bottom-4">
          {/* Header */}
          <div className="flex items-center justify-between px-5 py-3.5 bg-gradient-to-r from-sky-600 to-cyan-500 text-white shrink-0">
            <div className="flex items-center gap-3">
              <div className="w-9 h-9 rounded-full bg-white/20 flex items-center justify-center">
                <Bot size={20} />
              </div>
              <div>
                <p className="font-semibold text-sm">Ana — Recepcionista Virtual</p>
                <p className="text-xs text-sky-100">{clinicaNombre}</p>
              </div>
            </div>
            <button
              onClick={() => setIsOpen(false)}
              className="w-8 h-8 rounded-full hover:bg-white/20 flex items-center justify-center transition"
              aria-label="Cerrar chat"
            >
              <X size={16} />
            </button>
          </div>

          {/* Messages */}
          <div className="flex-1 overflow-y-auto px-4 py-4 space-y-3 bg-gray-50/50">
            {messages.map((msg, i) => (
              <div
                key={i}
                className={`flex items-end gap-2 ${msg.role === 'user' ? 'flex-row-reverse' : 'flex-row'}`}
              >
                {/* Avatar */}
                <div
                  className={`w-7 h-7 rounded-full flex items-center justify-center shrink-0 ${
                    msg.role === 'user'
                      ? 'bg-sky-100 text-sky-600'
                      : 'bg-gradient-to-br from-sky-500 to-cyan-400 text-white'
                  }`}
                >
                  {msg.role === 'user' ? <User size={14} /> : <Bot size={14} />}
                </div>

                {/* Bubble */}
                <div
                  className={`max-w-[75%] rounded-2xl px-4 py-2.5 text-sm leading-relaxed ${
                    msg.role === 'user'
                      ? 'bg-gradient-to-r from-sky-600 to-cyan-500 text-white rounded-br-md'
                      : 'bg-white border border-gray-100 text-gray-700 shadow-sm rounded-bl-md'
                  }`}
                >
                  {msg.content.split('**').map((part, idx) =>
                    idx % 2 === 1 ? (
                      <strong key={idx}>{part}</strong>
                    ) : (
                      <span key={idx}>{part}</span>
                    ),
                  )}
                </div>
              </div>
            ))}

            {/* Typing indicator */}
            {isSending && (
              <div className="flex items-end gap-2">
                <div className="w-7 h-7 rounded-full bg-gradient-to-br from-sky-500 to-cyan-400 text-white flex items-center justify-center shrink-0">
                  <Bot size={14} />
                </div>
                <div className="bg-white border border-gray-100 rounded-2xl rounded-bl-md px-4 py-3 shadow-sm">
                  <div className="flex gap-1">
                    <span className="w-2 h-2 bg-gray-300 rounded-full animate-bounce [animation-delay:0ms]" />
                    <span className="w-2 h-2 bg-gray-300 rounded-full animate-bounce [animation-delay:150ms]" />
                    <span className="w-2 h-2 bg-gray-300 rounded-full animate-bounce [animation-delay:300ms]" />
                  </div>
                </div>
              </div>
            )}

            <div ref={messagesEndRef} />
          </div>

          {/* Input */}
          <div className="px-4 py-3 border-t border-gray-100 bg-white shrink-0">
            <div className="flex items-center gap-2">
              <input
                ref={inputRef}
                type="text"
                value={input}
                onChange={(e) => setInput(e.target.value)}
                onKeyDown={handleKeyDown}
                placeholder="Escribe tu mensaje..."
                disabled={isSending}
                className="flex-1 border border-gray-200 rounded-full px-4 py-2.5 text-sm focus:ring-2 focus:ring-sky-500/20 focus:border-sky-500 transition outline-none disabled:opacity-50"
              />
              <button
                onClick={handleSend}
                disabled={!input.trim() || isSending}
                className="w-10 h-10 rounded-full bg-gradient-to-r from-sky-600 to-cyan-500 text-white flex items-center justify-center hover:shadow-lg hover:shadow-sky-500/25 transition-all disabled:opacity-40 disabled:hover:shadow-none shrink-0"
                aria-label="Enviar mensaje"
              >
                {isSending ? <Loader2 size={16} className="animate-spin" /> : <Send size={16} />}
              </button>
            </div>
            <p className="text-[10px] text-gray-300 text-center mt-2">
              Powered by AI · Las respuestas son orientativas
            </p>
          </div>
        </div>
      )}
    </>
  );
}
