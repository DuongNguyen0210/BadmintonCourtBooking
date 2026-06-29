import { apiClient } from './client'
import type {
  ConfirmPaymentResponse,
  JoinRequest,
  JoinRequestStatus,
} from '../types/joinRequest'

export async function requestToJoin(playSessionPostId: string) {
  const response = await apiClient.post<JoinRequest>(
    `/api/play-sessions/${playSessionPostId}/join-requests`,
  )
  return response.data
}

export async function getMyJoinRequests() {
  const response = await apiClient.get<JoinRequest[]>('/api/join-requests/mine')
  return response.data
}

export async function confirmJoinRequestPayment(joinRequestId: string) {
  const response = await apiClient.post<ConfirmPaymentResponse>(
    `/api/join-requests/${joinRequestId}/confirm-payment`,
  )
  return response.data
}

export async function getHostJoinRequests(status?: JoinRequestStatus) {
  const response = await apiClient.get<JoinRequest[]>('/api/host/join-requests', {
    params: status ? { status } : undefined,
  })
  return response.data
}

export async function approveJoinRequest(joinRequestId: string) {
  const response = await apiClient.post<JoinRequest>(
    `/api/host/join-requests/${joinRequestId}/approve`,
  )
  return response.data
}

export async function rejectJoinRequest(joinRequestId: string) {
  const response = await apiClient.post<JoinRequest>(
    `/api/host/join-requests/${joinRequestId}/reject`,
  )
  return response.data
}
