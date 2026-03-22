// 

using static SilkySouls.GameIds.Emevd;

namespace SilkySouls.Interfaces;

public interface IEmevdService
{
    void ExecuteEmevdCommand(EmevdCommand command);
}