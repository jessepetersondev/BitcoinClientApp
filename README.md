# Bitcoin Client App

A custom Bitcoin wallet application built with Blazor Server and .NET 8, providing various Bitcoin-related functionalities including address generation, transaction management, and compliance features.

## Features

- **User Authentication**: Register, login, and secure user accounts
- **Bitcoin Address Management**: Generate and manage Bitcoin addresses
- **Wallet Dashboard**: View wallet balance and addresses
- **Transaction Capabilities**: Create, sign, and manage Bitcoin transactions
- **PSBT Support**: Create and manage Partially Signed Bitcoin Transactions
- **Compliance Tools**: Check transactions against compliance requirements
- **Watch-Only Wallet Support**: Manage watch-only wallets using xpubs

## Technology Stack

- **Framework**: .NET 8.0, Blazor Server
- **Database**: SQLite with Entity Framework Core
- **Bitcoin Libraries**: NBitcoin (v8.0.8)
- **Custom Bitcoin Toolkits**:
  - BitcoinAddressToolkit - For address generation and management
  - TransactionsToolkit - For transaction building and signing
  - BtcComplianceToolkit - For regulatory compliance functions
  - PSBTToolkit - For PSBT management
  - WatchOnlyWalletToolkit - For watch-only wallet functions

## Project Structure

```
src/BitcoinClientApp/
├── Data/           # Database contexts and data access
├── Interfaces/     # Interface definitions
├── Models/         # Data models (User, Wallet, etc.)
├── Pages/          # Blazor pages (Dashboard, Address, etc.)
├── Services/       # Business logic services
├── Shared/         # Shared Blazor components
└── wwwroot/        # Static assets
```

### Key Components

- **Services**:
  - `AddressService`: Handles Bitcoin address generation and management
  - `AuthService`: Manages authentication and user accounts
  - `ComplianceService`: Provides compliance-checking functionality
  - `CryptoService`: Handles cryptographic operations
  - `PsbtService`: Manages PSBT creation and signing
  - `TransactionService`: Builds and signs Bitcoin transactions
  - `WatchOnlyService`: Handles watch-only wallet functionality

- **Pages**:
  - `Dashboard.razor`: Main wallet dashboard showing balance and addresses
  - `Address.razor`: Address generation interface
  - `Login.razor` & `Register.razor`: Authentication pages
  - `Transaction.razor`: Transaction creation interface

## Setup and Installation

### Prerequisites

- .NET 8.0 SDK
- Git

### Installation Steps

1. Clone the repository:
   ```
   git clone https://github.com/yourusername/BitcoinClientApp.git
   cd BitcoinClientApp
   ```

2. Make sure the required Bitcoin toolkit libraries are referenced:
   - BitcoinAddressToolkit
   - TransactionsToolkit
   - BtcComplianceToolkit
   - PSBTToolkit
   - WatchOnlyWalletToolkit

3. Restore dependencies:
   ```
   dotnet restore
   ```

4. Build the project:
   ```
   dotnet build
   ```

5. Run the application:
   ```
   cd src/BitcoinClientApp
   dotnet run
   ```

6. Access the application in your browser at `https://localhost:5001` or `http://localhost:5000`

## Testing

The project includes unit tests for all services and models. To run the tests:

```
cd tests/BitcoinClientApp.Tests
dotnet test
```

## Database

The application uses SQLite for data storage with Entity Framework Core for data access. The database schema includes:

- Users (authentication and user information)
- Wallets (Bitcoin address and wallet data)

## Security Considerations

- Private keys are encrypted using the `CryptoService` before being stored
- User passwords are hashed and not stored in plain text
- The application uses secure cryptographic functions from NBitcoin
