export class User {
  constructor(
    public readonly id: string,
    public readonly username: string,
    public readonly email: string,
    public readonly passwordHash: string,
    public readonly role: string,
    public readonly createdAt: Date,
    public readonly updatedAt: Date,
  ) {}

  public static create(
    id: string,
    username: string,
    email: string,
    passwordHash: string,
    role: string = 'user',
  ): User {
    const now = new Date();
    return new User(id, username, email, passwordHash, role, now, now);
  }
}
