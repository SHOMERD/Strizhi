using Strizhi.TelegramPart.logics;

class Program
{
    static async Task Main()
    {
		try
		{
            MainLogics mainLogics = new MainLogics();
            await Task.Delay(-1);
        }
		catch (Exception s)
		{
            FileСatcher.Loger("!!!!!!!!!!!! вылетел \n"+s.Message);
        }
         

    }


}
