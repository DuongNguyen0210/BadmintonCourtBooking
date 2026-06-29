import { apiClient } from './client'
import type { Notification } from '../types/notification'

export async function getNotifications() {
  const response = await apiClient.get<Notification[]>('/api/notifications')
  return response.data
}

export async function markNotificationAsRead(notificationId: string) {
  const response = await apiClient.patch<Notification>(
    `/api/notifications/${notificationId}/read`,
  )
  return response.data
}
