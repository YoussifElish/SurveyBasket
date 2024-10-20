
# SurveyBasket

SurveyBasket is a .NET-based application designed to simplify the creation, management, and execution of surveys and polls. It includes secure user authentication, role-based access control, and scheduling for background jobs to ensure a scalable solution for organizations to gather and analyze feedback effectively.

## Key Features

- **User Authentication & Authorization**: Implements JWT-based authentication with role-based access control to manage different user actions.
- **Survey & Poll Management**: Easily create, edit, and manage surveys and their questions using API endpoints.
- **Background Jobs**: Automates tasks using Hangfire, such as sending notifications for new polls.
- **Rate Limiting**: Protects the application with built-in rate limiting to prevent excessive requests.
- **Health Checks**: Monitors the health and performance of the application using integrated health checks.
- **Email Notifications**: Uses SMTP to send email notifications for account verification and poll updates.
- **Logging**: Employs Serilog for advanced logging with support for JSON formatting.
- **Input Validation**: Validates user inputs with FluentValidation to ensure data integrity.

## Project Structure

- **Abstractions**: Contains core utilities and abstractions like `Result`, `PaginatedList`, and constants for roles and permissions.
- **Authentication**: Manages JWT token creation, authentication filters, and role assignments.
- **Controllers**: API endpoints for surveys, user management, and roles.
- **Services**: Core services for handling surveys, email notifications, authentication, and background jobs.
- **Templates**: Predefined email templates for account confirmation and password resets.
- **Settings**: Configuration files for email, database, and JWT settings.

## Installation

1. **Clone the repository**:

    ```bash
    git clone https://github.com/yourusername/SurveyBasket.git
    cd SurveyBasket
    ```

2. **Configure settings**:

   Edit `appsettings.json` to configure:
   - **Database connection**: Update the `DefaultConnection` and `HangfireConnection` strings.
   - **JWT Settings**: Define the `Key`, `Issuer`, and `Audience`.
   - **Email Settings**: Configure the SMTP server credentials under `MailSettings`.

3. **Run the application**:

   Open the solution in Visual Studio or use the .NET CLI:

   ```bash
   dotnet run
   ```

4. **Set up Hangfire**:

   Hangfire dashboard can be accessed at `/jobs` for monitoring background tasks. Default credentials can be set in `appsettings.json` under `HangFireSettings`.

## Dependencies

- .NET 9+
- Entity Framework Core
- Hangfire
- Serilog
- FluentValidation

## License

This project is licensed under the MIT License. See the LICENSE file for details.

## Contributions

Contributions are welcome! Please fork the repository and create a pull request to contribute new features, bug fixes, or improvements.
