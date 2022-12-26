import { ApiError } from '@axis/lib/core';

export interface ApiResult<T> {
  statusCode: number,
  success: boolean,
  traceId: string,
  elapsed: number,
  data?: T,
  error?: ApiError,
}
