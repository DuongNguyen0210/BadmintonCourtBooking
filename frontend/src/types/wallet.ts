export type WalletResponse = {
  availableBalanceVnd: number
  heldBalanceVnd: number
}

export type WalletTransaction = {
  id: string
  type: string
  status: string
  amountVnd: number
  balanceBeforeVnd: number
  balanceAfterVnd: number
  description: string
  createdAtUtc: string
}

export type DevelopmentTopUpRequest = {
  amountVnd: number
}
