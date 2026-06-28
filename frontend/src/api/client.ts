import axios from 'axios'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7282'

export const apiClient = axios.create({
  baseURL: apiBaseUrl.replace(/\/$/, ''),
  withCredentials: true,
})
