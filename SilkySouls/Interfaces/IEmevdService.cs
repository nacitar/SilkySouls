// 

using static SilkySouls.GameIds.Emevd;

namespace SilkySouls.Interfaces;

public interface IEmevdService
{
    bool? ExecuteEmevdCommand(EmevdCommand command);
}