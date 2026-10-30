const API = import.meta.env.VITE_API_URL || 'http://localhost:5080/api'
export function getToken() { return localStorage.getItem('smartbank_token') }
export async function api(path, options = {}) {
  const headers = { 'Content-Type': 'application/json', ...(options.headers || {}) }
  const token = getToken()
  if (token) headers.Authorization = `Bearer ${token}`
  const response = await fetch(`${API}${path}`, { ...options, headers })
  const text = await response.text()
  const data = text ? JSON.parse(text) : null
  if (!response.ok) throw new Error(data?.message || data?.title || `Request failed (${response.status})`)
  return data
}
