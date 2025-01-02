using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using QuickFix.Logger;
using QuickFix.Store;
using Message = QuickFix.Message;

public class AcceptorApp : MessageCracker, IApplication
{
    public void FromApp(Message msg, SessionID sessionID)
    {
        Console.WriteLine("IN:  " + msg);
        Crack(msg, sessionID);
    }

    public void OnCreate(SessionID sessionID)
    {
        Console.WriteLine("Session - " + sessionID);
    }

    public void OnLogout(SessionID sessionID)
    {
        Console.WriteLine("Logout - " + sessionID);
    }

    public void OnLogon(SessionID sessionID)
    {
        Console.WriteLine("Logon - " + sessionID);
    }

    public void FromAdmin(Message msg, SessionID sessionID)
    {
        Console.WriteLine("IN: " + msg);
    }

    public void ToAdmin(Message msg, SessionID sessionID)
    {
        Console.WriteLine("OUT: " + msg);
    }

    public void ToApp(Message msg, SessionID sessionID)
    {
        Console.WriteLine("OUT: " + msg);
    }

    public void OnMessage(QuickFix.FIX44.News n, SessionID s)
    {
        try
        {
            ParseNewsMessage(n);
            SendAck(s);
        }
        catch (SessionNotFound ex)
        {
            Console.WriteLine("==session not found exception!==");
            Console.WriteLine(ex.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
    
    private void SendAck(SessionID s)
    {
        News n = new News();
        n.Headline = new Headline("Acknowledgement");
            
        News.LinesOfTextGroup textGroup = new News.LinesOfTextGroup();
        textGroup.Text = new Text("Got it");
        n.AddGroup(textGroup);
        n.LinesOfText = new LinesOfText(1);
            
        Session.SendToTarget(n, s);
    }
    
    void ParseNewsMessage(News n)
    {
        if (n.IsSetHeadline())
        {
            string headline = n.Headline.getValue();
            Console.WriteLine($"Headline: {headline}");
        }
        else
        {
            Console.WriteLine("Headline is missing.");
        }

        if (n.IsSetField(LinesOfText.TAG))
        {
            int numOfLines = n.GetInt(LinesOfText.TAG);
            for (int i = 1; i <= numOfLines; i++)
            {
                try
                {
                    News.LinesOfTextGroup textGroup = new News.LinesOfTextGroup();
                    n.GetGroup(i, textGroup);

                    if (textGroup.IsSetText())
                    {
                        string line = textGroup.Text.getValue();
                        Console.WriteLine($"Line {i}: {line}");
                    }
                    else
                    {
                        Console.WriteLine($"Line {i} has no text.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading line {i}: {ex.Message}");
                }
            }
        }
        else
        {
            Console.WriteLine("No LinesOfText found in the message.");
        }
    }
}

public class MyApp
{
    private const string HttpServerPrefix = "http://127.0.0.1:5080/";
    static void Main(string[] args)
    {
        try
        {
            AcceptorApp acceptorApp = new AcceptorApp();
            SessionSettings settings = new SessionSettings(args[0]);
            IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
            ILogFactory logFactory = new FileLogFactory(settings);
            ThreadedSocketAcceptor acceptor = new ThreadedSocketAcceptor(
                acceptorApp,
                storeFactory,
                settings,
                logFactory);
            HttpServer srv = new HttpServer(HttpServerPrefix, settings);

            acceptor.Start();
            srv.Start();
            
            Console.WriteLine("Acceptor Started");
            Console.WriteLine("Running HTTP Server On: " + HttpServerPrefix);
            Console.WriteLine("Press <enter> to quit");
            Console.Read();
            
            srv.Stop();
            acceptor.Stop();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}