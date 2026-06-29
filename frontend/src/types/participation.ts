export type CancellationRefundChoice = 'StandardRefund' | 'WaiveRefund'

export type CancelParticipationRequest = {
  refundChoice: CancellationRefundChoice
  reason?: string | null
  waiveRefundConfirmation?: string | null
}

export type CancellationResponse = {
  cancellationId: string
  participantId: string
  originalAmountVnd: number
  refundAmountVnd: number
  cancellationFeeVnd: number
  refundChoice: CancellationRefundChoice
  wallet: {
    availableBalanceVnd: number
    heldBalanceVnd: number
  }
}
