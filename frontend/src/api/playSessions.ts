import { apiClient } from './client'
import type {
  CreatePlaySessionPostRequest,
  PlaySessionPostDetail,
  PlaySessionPostListItem,
  UpdatePlaySessionPostRequest,
} from '../types/playSession'

export async function getPlaySessionPosts() {
  const response = await apiClient.get<PlaySessionPostListItem[]>('/api/play-sessions')
  return response.data
}

export async function getPlaySessionPost(id: string) {
  const response = await apiClient.get<PlaySessionPostDetail>(`/api/play-sessions/${id}`)
  return response.data
}

export async function createPlaySessionPost(request: CreatePlaySessionPostRequest) {
  const response = await apiClient.post<PlaySessionPostDetail>('/api/play-sessions', request)
  return response.data
}

export async function updatePlaySessionPost(
  id: string,
  request: UpdatePlaySessionPostRequest,
) {
  const response = await apiClient.put<PlaySessionPostDetail>(
    `/api/play-sessions/${id}`,
    request,
  )
  return response.data
}

export async function cancelPlaySessionPost(id: string) {
  await apiClient.delete(`/api/play-sessions/${id}`)
}
