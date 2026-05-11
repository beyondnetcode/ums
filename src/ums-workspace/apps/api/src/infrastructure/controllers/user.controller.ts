import {
  Controller,
  Post,
  Body,
  Get,
  UsePipes,
  ValidationPipe,
  HttpCode,
  HttpStatus,
} from '@nestjs/common';
import { RegisterUserUseCase } from '../../application/use-cases/register-user.use-case';
import { RegisterUserDto } from '../../application/dtos/register-user.dto';

@Controller('users')
export class UserController {
  constructor(private readonly registerUserUseCase: RegisterUserUseCase) {}

  @Post('register')
  @HttpCode(HttpStatus.CREATED)
  @UsePipes(new ValidationPipe({ whitelist: true, forbidNonWhitelisted: true }))
  public async register(@Body() dto: RegisterUserDto) {
    const user = await this.registerUserUseCase.execute(dto);
    // Return sanitized data - do not expose password hashes!
    return {
      success: true,
      message: 'User registered successfully.',
      data: {
        id: user.id,
        username: user.username,
        email: user.email,
        role: user.role,
        createdAt: user.createdAt,
      },
    };
  }

  @Get()
  public async getSample() {
    return {
      message:
        'UMS API is fully operational following Clean Architecture and OWASP security practices.',
      status: 'healthy',
      timestamp: new Date().toISOString(),
    };
  }
}
