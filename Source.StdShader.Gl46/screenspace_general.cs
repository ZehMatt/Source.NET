using Source.Common.MaterialSystem;

namespace Source.StdShader.Gl46;

public class screenspace_general : BaseVSShader
{
	public static string HelpString = "Help for screenspace_general";
	public static int Flags = 0;

	public override string? GetFallbackShader(IMaterialVar[] vars) => "screenspace_general_dx9";
}
