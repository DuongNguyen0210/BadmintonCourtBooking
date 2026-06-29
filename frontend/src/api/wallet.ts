import { apiClient } from './client'
import type {
  DevelopmentTopUpRequest,
  WalletResponse,
  WalletTransaction,
} from '../types/wallet'

export async function getWallet() {
  const response = await apiClient.get<WalletResponse>('/api/wallet')
  return response.data
}

export async function getWalletTransactions() {
  const response = await apiClient.get<WalletTransaction[]>('/api/wallet/transactions')
  return response.data
}

export async function topUpDevelopmentWallet(request: DevelopmentTopUpRequest) {
  const response = await apiClient.post<WalletResponse>(
    '/api/development/wallet/top-up',
    request,
  )
  return response.data
}
