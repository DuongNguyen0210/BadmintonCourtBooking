export type PostStatus = 'Active' | 'Filled' | 'Completed' | 'Cancelled' | 'Expired'

export type CreatePlaySessionPostRequest = {
  title: string
  description: string
  courtName: string
  courtAddress: string
  startTime: string
  endTime: string
  pricePerPlayer: number
  maxPlayers: number
  currentPlayers: number
  malePlayers: number
  femalePlayers: number
  showMalePlayers: boolean
  showFemalePlayers: boolean
}

export type UpdatePlaySessionPostRequest = CreatePlaySessionPostRequest

export type PlaySessionPostListItem = {
  id: string
  title: string
  courtName: string
  courtAddress: string
  startTime: string
  endTime: string
  pricePerPlayer: number
  maxPlayers: number
  currentPlayers: number
  malePlayers: number | null
  femalePlayers: number | null
  status: PostStatus
  creatorName: string
  canManage: boolean
}

export type PlaySessionPostDetail = PlaySessionPostListItem & {
  description: string
  showMalePlayers: boolean
  showFemalePlayers: boolean
  creatorUserId: string
  createdAt: string
  updatedAt: string | null
}
