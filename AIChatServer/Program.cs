using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Server
{
    static void Main(string[] args)
    {
        TcpListener server = null;
        try
        {
            // Устанавливаем порт и IP-адрес для сервера
            int port = 8888;
            IPAddress localAddr = IPAddress.Parse("192.168.100.14");

            // Создаем TcpListener для прослушивания входящих соединений
            server = new TcpListener(localAddr, port);

            // Запускаем сервер
            server.Start();

            Console.WriteLine("Ожидание подключений...");

            while (true)
            {
                // Принимаем входящее соединение
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Подключен клиент.");

                // Создаем поток для обработки клиента
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                clientThread.Start(client);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            if (server != null)
                server.Stop();
        }
    }

    private static void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();

        byte[] buffer = new byte[1024];
        int bytesRead;

        try
        {
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                // Преобразуем полученные данные в строку
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Получено: " + data);

                // Отправляем ответ клиенту
                byte[] response = Encoding.UTF8.GetBytes("Сообщение получено: " + data);
                stream.Write(response, 0, response.Length);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            client.Close();
        }
    }
}