import type { WalletResponse } from './wallet'

export type JoinRequestStatus =
  | 'PendingHostApproval'
  | 'AwaitingPayment'
  | 'Joined'
  | 'Rejected'
  | 'Cancelled'
  | 'Expired'

export type JoinRequest = {
  id: string
  playSessionPostId: string
  playSessionTitle: string
  courtName: string
  guestUserId: string
  guestName: string
  status: JoinRequestStatus
  pricePerPlayerVnd: number
  requestedAtUtc: string
  reviewedAtUtc: string | null
  paymentDueAtUtc: string | null
  paidAtUtc: string | null
  cancelledAtUtc: string | null
}

export type ConfirmPaymentResponse = {
  participantId: string
  joinRequest: JoinRequest
  wallet: WalletResponse
}
