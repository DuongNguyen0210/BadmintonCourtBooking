import { apiClient } from './client'
import type {
  CancelParticipationRequest,
  CancellationResponse,
} from '../types/participation'

export async function cancelParticipation(
  participantId: string,
  request: CancelParticipationRequest,
) {
  const response = await apiClient.post<CancellationResponse>(
    `/api/participations/${participantId}/cancel`,
    request,
  )
  return response.data
}
