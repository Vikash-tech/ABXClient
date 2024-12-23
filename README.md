# ABX Mock Exchange Client

The ABX Mock Exchange Client is a C# application that connects to a mock exchange server. The client receives and processes stock trade data packets, validates the sequence of the data, and outputs the data to a JSON file.

## Features

- **Receive packets from a server**: The client communicates with a server to receive stock trade packets.
- **Stock packet handling**: It ensures the stock packets are processed in the correct order.
- **Sequence validation**: If there are missing packets in the sequence, they are automatically filled with placeholders.
- **Data saving**: The processed packets are saved in a JSON file for future use.

## Requirements

- .NET Core (or .NET Framework) for building and running the application.
- TCP server running to send stock trade data packets (mock exchange server).

## How to Use

1. **Clone the repository**:

   ```bash
   git clone https://github.com/your-username/ABXClient.git
   cd ABXClient
