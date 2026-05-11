/**
 * Result Pattern (Either Monad)
 * Implements ADR 0019: Tactical Design Patterns for Domain Integrity.
 * 
 * This class ensures that the Domain layer NEVER throws HTTP-specific exceptions.
 * It encapsulates a successful value or a failure error, allowing infrastructure adapters
 * (like NestJS HTTP Controllers or Dapr gRPC endpoints) to interpret the result without
 * the core being coupled to the delivery mechanism.
 */

export class Result<T, E = string> {
  private readonly _isSuccess: boolean;
  private readonly _isFailure: boolean;
  private readonly _error: E | null;
  private readonly _value: T | null;

  private constructor(isSuccess: boolean, error?: E | null, value?: T | null) {
    if (isSuccess && error) {
      throw new Error('InvalidOperation: A result cannot be successful and contain an error');
    }
    if (!isSuccess && !error) {
      throw new Error('InvalidOperation: A failing result needs to contain an error message');
    }

    this._isSuccess = isSuccess;
    this._isFailure = !isSuccess;
    this._error = error || null;
    this._value = value !== undefined ? value : null;

    Object.freeze(this);
  }

  public getValue(): T {
    if (!this._isSuccess) {
      throw new Error("Can't get the value of an error result. Use 'errorValue' instead.");
    }
    return this._value as T;
  }

  public get errorValue(): E {
    return this._error as E;
  }

  public get isSuccess(): boolean {
    return this._isSuccess;
  }

  public get isFailure(): boolean {
    return this._isFailure;
  }

  public static ok<U>(value?: U): Result<U> {
    return new Result<U>(true, null, value);
  }

  public static fail<U, E>(error: E): Result<U, E> {
    return new Result<U, E>(false, error);
  }

  public static combine(results: Result<any>[]): Result<any> {
    for (const result of results) {
      if (result.isFailure) return result;
    }
    return Result.ok();
  }
}
