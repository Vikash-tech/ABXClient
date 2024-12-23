using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Net;
using Newtonsoft.Json;

namespace ABXClient
{
    public class StockPacket
    {
        public string Symbol { get; set; }
        public char BuySellIndicator { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        public int Sequence { get; set; }
    }

    class ABXClient
    {
        private const string ServerAddress = "127.0.0.1"; // Server IP address
        private const int Port = 3000; // Server Port

        static void Main(string[] args)
        {
            Console.WriteLine("ABX Mock Exchange Client");

            try
            {
                using (var tcpClient = new TcpClient(ServerAddress, Port))
                using (var stream = tcpClient.GetStream())
                {
                    SendRequest(stream, 1, 0);
                    List<StockPacket> stockPackets = ReceivePackets(stream);

                    //List<StockPacket> validPackets = EnsureContinuousSequences(stockPackets); Commented Because Loop Run End Time So 

                    // Save to JSON file
                    SaveToJsonFile(stockPackets);
                    Console.WriteLine("Data saved to output.json");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static void SendRequest(NetworkStream stream, byte callType, byte resendSeq)
        {
            byte[] requestPayload = new byte[2];
            requestPayload[0] = callType;
            requestPayload[1] = resendSeq;

            stream.Write(requestPayload, 0, requestPayload.Length);
            Console.WriteLine($"Sent request: CallType = {callType}, ResendSeq = {resendSeq}");
        }
        // Receive the packets from the server
        private static List<StockPacket> ReceivePackets(NetworkStream stream)
        {
            List<StockPacket> packets = new List<StockPacket>();
            byte[] buffer = new byte[16]; 
            int bytesReadTotal = 0; 
            while (true)
            {
                int bytesRead = 0;
                while (bytesRead < buffer.Length)
                {
                    int readNow = stream.Read(buffer, bytesRead, buffer.Length - bytesRead);
                    if (readNow == 0) break;

                    bytesRead += readNow;
                }

                if (bytesRead == 0) break; 

                try
                {
                    StockPacket packet = ParsePacket(buffer);
                    packets.Add(packet);
                    Console.WriteLine($"Received packet: {packet.Sequence} - {packet.Symbol} {packet.BuySellIndicator} {packet.Quantity} at {packet.Price}");
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Error parsing packet: {ex.Message}. Skipping this packet.");
                }
            }
            return packets;
        }
        private static StockPacket ParsePacket(byte[] buffer)
        {
            if (buffer.Length < 16)
            {
                throw new ArgumentException("Buffer is too small to parse a full packet. Expected 16 bytes.");
            }
            string symbol = Encoding.ASCII.GetString(buffer, 0, 4).Trim();

            char buySellIndicator = (char)buffer[4];

            int quantity = BitConverter.ToInt32(buffer, 5);
            quantity = IPAddress.NetworkToHostOrder(quantity);

            int price = BitConverter.ToInt32(buffer, 9);
            price = IPAddress.NetworkToHostOrder(price);

            int sequence = BitConverter.ToInt32(buffer,10);
            sequence = IPAddress.NetworkToHostOrder(sequence);

            return new StockPacket
            {
                Symbol = symbol,
                BuySellIndicator = buySellIndicator,
                Quantity = quantity,
                Price = price,
                Sequence = sequence
            };
        }
        private static List<StockPacket> EnsureContinuousSequences(List<StockPacket> packets)
        {
            try
            {
                // Sort packets by sequence number to ensure they're processed in order
                packets.Sort((p1, p2) => p1.Sequence.CompareTo(p2.Sequence));

                int expectedSequence = 1; // Start with the first expected sequence
                List<StockPacket> validPackets = new List<StockPacket>();

                foreach (var packet in packets)
                {
                    Console.WriteLine($"Processing packet: {packet.Sequence} - {packet.Symbol}");

                    // Handle missing sequences
                    while (packet.Sequence > expectedSequence)
                    {
                        Console.WriteLine($"Missing packet detected: Expected {expectedSequence}, adding placeholder.");

                        validPackets.Add(new StockPacket
                        {
                            Sequence = expectedSequence,
                            Symbol = "N/A", 
                            BuySellIndicator = 'N',  
                            Quantity = 0,    
                            Price = 0     
                        });
                        expectedSequence++;
                    }


                    validPackets.Add(packet);
                    Console.WriteLine($"Added valid packet: {packet.Sequence} - {packet.Symbol}");

                    expectedSequence++;
                }

                return validPackets;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while ensuring continuous sequences: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw new InvalidOperationException("An error occurred while ensuring continuous sequences.", ex);
            }
        }
        private static void SaveToJsonFile(List<StockPacket> packets)
        {
            string json = JsonConvert.SerializeObject(packets, Formatting.Indented);

            string filePath = Path.Combine("C:\\Users\\Vikash Gautam\\Downloads", "output.json");

            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(filePath, json);
                Console.WriteLine($"Data saved to {filePath}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Failed to save to file: {ex.Message}");
            }
        }
    }
}
