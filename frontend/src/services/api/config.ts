// URL base de la API. NEXT_PUBLIC_* se incrusta en el build (en Docker llega como build arg).
export const API_URL = (process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8080").replace(/\/$/, "");
