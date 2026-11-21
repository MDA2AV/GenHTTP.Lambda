using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Data;

/// <summary>
/// What a stored tier or view becomes when it is read: the member that has
/// its number, or - where no member has it, a row of a later version or one
/// written by hand - the least it could mean.
/// </summary>
/// <remarks>
/// A tier no member has is the sandbox, which allows the least, and a view
/// the full one - as the migration that numbered the columns read a name it
/// did not know (V30 of the platform's migrations, V5 in PostgreSQL). The
/// columns that may be empty - a request to connect, an origin, an ending -
/// read as the number they hold: the rules treat a number no member has as
/// nothing decided (<c>ConnectivityRule</c>), and the names as nothing
/// recorded (<see cref="ConnectivityStatusNames"/> and the like), which is
/// what an empty one means. EF converts no nulls itself.
/// </remarks>
internal static class StoredValues
{

    public static LambdaTier Tier(short number)
        => Enum.IsDefined((LambdaTier)number) ? (LambdaTier)number : LambdaTier.Sandbox;

    public static EditorView View(short number)
        => Enum.IsDefined((EditorView)number) ? (EditorView)number : EditorView.Full;

}
