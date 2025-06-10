using Midas.Presentation.Data;
using Midas.Presentation.Game;

public sealed class CashChingController : IPresentationController
{
    public static CashChingStatus Data { get; private set; }

	public CashChingController()
	{
		Data = new CashChingStatus();
		StatusDatabase.AddStatusBlock(Data);
	}

	public void Init() { }
	public void DeInit() { }
	public void Destroy()
	{
		StatusDatabase.RemoveStatusBlock(Data);
		Data = null;
	}
}
