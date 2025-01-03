using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;
using Message = QuickFix.Message;

public class InitiatorApp : MessageCracker, IApplication
{
    private Random rng = new Random();

    public void FromApp(Message msg, SessionID sessionID)
    {
        // Console.WriteLine("IN:  " + msg);
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
        this.SendNews(sessionID);
    }

    public void FromAdmin(Message msg, SessionID sessionID)
    {
        // Console.WriteLine("IN: " + msg);
    }

    public void ToAdmin(Message msg, SessionID sessionID)
    {
        // Console.WriteLine("OUT: " + msg);
    }

    public void ToApp(Message msg, SessionID sessionID)
    {
        // Console.WriteLine("OUT: " + msg);
    }

    public void OnMessage(QuickFix.FIX44.News n, SessionID s)
    {
        try
        {
            ParseAck(n);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }
    
    private void SendMessage(Message m, SessionID s)
    {
        Session.SendToTarget(m, s);
    }

    private void SendNews(SessionID s)
    {
        int temp  = this.rng.Next(-27, 37);
        News n = new News();
        n.Headline = new Headline("Breaking news!");
        
        News.LinesOfTextGroup textGroup = new News.LinesOfTextGroup();
        textGroup.Text = new Text("Hank: Nothing is happening, it's so boring...");
        n.AddGroup(textGroup);

        textGroup = new News.LinesOfTextGroup();
        textGroup.Text = new Text("Hank: Now here's Tom with the weather!");
        n.AddGroup(textGroup);

        textGroup = new News.LinesOfTextGroup();
        textGroup.Text = new Text("Tom: It's " + temp + "F outside, man that's cold... Why is it always so cold in Chicago?");
        n.AddGroup(textGroup);
        
        this.SendMessage(n, s);
    }
    
    void ParseAck(News n)
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
    static void Main(string[] args)
    {
        try
        {
            SessionSettings settings = new SessionSettings(args[0]);
            InitiatorApp initiatorApp = new InitiatorApp();
            IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
            ILogFactory logFactory = new FileLogFactory(settings);
            SocketInitiator initiator = new SocketInitiator(
                initiatorApp,
                storeFactory,
                settings,
                logFactory);

            initiator.Start();
            
            Console.WriteLine("Initiator Started");
            Console.WriteLine("Press <enter> to quit");
            Console.Read();
            
            initiator.Stop();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}