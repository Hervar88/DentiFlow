# 🦷 Proyecto: DentiFlow- SaaS para Odontólogos

Este documento sirve como guía técnica y hoja de ruta para el desarrollo del MVP. 
**Stack:** .NET 10 (Backend), React (Frontend), PostgreSQL (DB), DigitalOcean (Cloud).

---

## 🏗️ Fase 1: Configuración de Base y Docker
*Objetivo: Tener el entorno de desarrollo listo y contenedorizado.*

1.  **Estructura de Carpetas:**
    - `/src/api`: Proyecto Web API .NET 10.
    - `/src/web`: Proyecto React + Vite + TypeScript.
    - `/docker`: Archivos de configuración de Docker.

2.  **Docker Compose Inicial:**
    - Configurar un `docker-compose.yml` que levante:
        - `db`: Imagen de PostgreSQL.
        - `api`: Dockerfile de .NET 10.
        - `web`: Dockerfile de Node.js.

> **💡 Tip Copilot:** Abre el archivo vacío y escribe: 
> `// Generate a docker-compose.yml for a .NET 8 API, a React frontend, and a PostgreSQL database.`

---

## 🛠️ Fase 2: Backend (.NET 8) y Base de Datos
*Objetivo: Definir el núcleo del negocio.*

1.  **Arquitectura:** Usar *Clean Architecture* o *Vertical Slices* para escalabilidad.
2.  **Entidades Principales (EF Core):**
    - `Clínica`: Nombre, especialidades, logo (URL de Spaces).
    - `Dentista`: Datos, especialidad, GoogleCalendarToken.
    - `Paciente`: Datos de contacto, historial básico.
    - `Cita`: Fecha, estado, ID de pago (Mercado Pago), SyncID de Google.
3.  **Endpoints Core:**
    - `GET /clinica/profile`: Datos de la landing.
    - `POST /appointments/book`: Agendar cita.

---

## 🎨 Fase 3: Frontend y Landing Page (React)
*Objetivo: Interfaz para el dentista y página pública para pacientes.
Generate a professional landing page for a dental clinic called 'DentiFlow' using Tailwind CSS, including a hero section and a services grid.*

1.  **Dashboard del Dentista:**
    - Vista de calendario (usar librerías como `FullCalendar` o `React-Big-Calendar`).
    - Gestión de pacientes.
2.  **Landing Page Dinámica:**
    - Una ruta `/clinica/[slug]` que cargue los datos de la DB para que los pacientes agenden.
3.  **Integración de UI:** Usar **Tailwind CSS** + **Shadcn/UI** para componentes rápidos y profesionales.

---

## 🔌 Fase 4: Integraciones Clave (APIs Externas)

1.  **Google Calendar API:**
    - Configurar OAuth2 para que el dentista vincule su cuenta.
    - Webhooks para detectar si el dentista mueve una cita manualmente en Google.
2.  **Mercado Pago (México):**
    - Implementar el Checkout Pro para cobros de anticipos.
    - Webhook para marcar la cita como "Pagada".
3.  **Notificaciones:**
    - Integración con API de WhatsApp (o n8n como puente) para enviar recordatorios automáticos.

---

## 🤖 Fase 5: AI Chatbot (Receptionist) ✅
*Objetivo: Automatizar dudas y pre-agendado.*

1.  **Motor de IA:** ✅ OpenAI (`gpt-4o-mini`) via `IChatbotService` / `OpenAiChatbotService`.
2.  **RAG (Retrieval-Augmented Generation):** ✅ `ChatService` construye system prompt dinámico con datos reales de la clínica (nombre, dirección, teléfono, especialidades, equipo médico, horarios, métodos de pago).
3.  **Widget de Chat:** ✅ `ChatWidget.tsx` flotante en la landing page con burbuja animada, panel expandible, historial de conversación, indicador de escritura y manejo graceful de errores.
4.  **Endpoints:** `POST /chat/{slug}` (stateless) + `GET /chat/configured`.

---

## ☁️ Fase 6: Despliegue en DigitalOcean (Droplet)
*Objetivo: Producción a bajo costo.*

1.  **Configuración del Droplet:**
    - Instalar Docker y Docker Compose.
    - Configurar **Nginx** como Proxy Inverso.
2.  **Certificados:** Usar Let's Encrypt (Certbot) para HTTPS gratuito.
3.  **Spaces:** Configurar el SDK de AWS (compatible con Spaces) en .NET para subir radiografías/fotos.

---

## 📝 Notas de Copilot para el Desarrollador
- **Para Generar Modelos:** Selecciona el esquema de tu DB y dile a Copilot: `@workspace /generate C# EF Core classes for this schema`.
- **Para Integrar Mercado Pago:** `// How to create a Preference in Mercado Pago SDK for .NET 10?`
- **Para el Chatbot:** `// Create a service in .NET to send a prompt to Gemini API with system instructions for a dental receptionist.`