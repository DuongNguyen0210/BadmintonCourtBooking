export type Notification = {
  id: string
  type: string
  title: string
  message: string
  relatedEntityId: string | null
  isRead: boolean
  createdAtUtc: string
  readAtUtc: string | null
}
