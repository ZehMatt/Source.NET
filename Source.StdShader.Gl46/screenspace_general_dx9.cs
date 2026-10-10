using Source.Common.Bitmap;
using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;
using Source.Common.ShaderLib;

namespace Source.StdShader.Gl46;

public class screenspace_general_dx9 : BaseVSShader
{
	public static string HelpString = "Help for screenspace_general";
	public static int Flags = (int)ShaderParamFlags.NotEditable;
	public static List<ShaderParam> ShaderParams = [];

	public class ShaderParam
	{
		public readonly ShaderParamInfo Info;
		public readonly int Index;
		public ShaderParam(string name, ShaderParamType type, ReadOnlySpan<char> defaultParam, ReadOnlySpan<char> help, int flags = 0) {
			Info.Name = name;
			Info.Type = type;
			Info.DefaultValue = new(defaultParam);
			Info.Help = new(help);
			Info.Flags = (ShaderParamFlags)flags;
			Index = (int)ShaderMaterialVars.Count + ShaderParams.Count;
			ShaderParams.Add(this);
		}
		public static implicit operator int(ShaderParam param) => param.Index;
		public ReadOnlySpan<char> GetName() => Info.Name;
		public ShaderParamType GetType() => Info.Type;
		public ReadOnlySpan<char> GetDefaultValue() => Info.DefaultValue;
		public int GetFlags() => (int)Info.Flags;
		public ReadOnlySpan<char> GetHelp() => Info.Help;
	}

	public static readonly ShaderParam C0_X = new($"${nameof(C0_X)}", ShaderParamType.Float, "0", "");
	public static readonly ShaderParam C0_Y = new($"${nameof(C0_Y)}", ShaderParamType.Float, "0", "");
	public static readonly ShaderParam C0_Z = new($"${nameof(C0_Z)}", ShaderParamType.Float, "0", "");
	public static readonly ShaderParam C0_W = new($"${nameof(C0_W)}", ShaderParamType.Float, "0", "");
	public static readonly ShaderParam C1_X = new($"${nameof(C1_X)}", ShaderParamType.Float, "0", "");
	public static readonly ShaderParam C1_Y = new($"${nameof(C1_Y)}", ShaderParamType.Float, "0", "");
	public static readonly ShaderParam C1_Z = new($"${nameof(C1_Z)}", ShaderParamType.Float, "0", "");
	public static readonly ShaderParam C1_W = new($"${nameof(C1_W)}", ShaderParamType.Float, "0", "");
	public static readonly ShaderParam C2_X = new($"${nameof(C2_X)}", ShaderParamType.Float, "0", "");
	public static readonly ShaderParam C2_Y = new($"${nameof(C2_Y)}", ShaderParamType.Float, "0", "");
	public static readonly ShaderParam C2_Z = new($"${nameof(C2_Z)}", ShaderParamType.Float, "0", "");
	public static readonly ShaderParam C2_W = new($"${nameof(C2_W)}", ShaderParamType.Float, "0", "");
	public static readonly ShaderParam C3_X = new($"${nameof(C3_X)}", ShaderParamType.Float, "0", "");
	public static readonly ShaderParam C3_Y = new($"${nameof(C3_Y)}", ShaderParamType.Float, "0", "");
	public static readonly ShaderParam C3_Z = new($"${nameof(C3_Z)}", ShaderParamType.Float, "0", "");
	public static readonly ShaderParam C3_W = new($"${nameof(C3_W)}", ShaderParamType.Float, "0", "");
	public static readonly ShaderParam PIXSHADER = new($"${nameof(PIXSHADER)}", ShaderParamType.String, "", "Name of the pixel shader to use");
	public static readonly ShaderParam DISABLE_COLOR_WRITES = new($"${nameof(DISABLE_COLOR_WRITES)}", ShaderParamType.Integer, "0", "");
	public static readonly ShaderParam ALPHATESTED = new($"${nameof(ALPHATESTED)}", ShaderParamType.Float, "0", "");
	public static readonly ShaderParam ALPHA_BLEND_COLOR_OVERLAY = new($"${nameof(ALPHA_BLEND_COLOR_OVERLAY)}", ShaderParamType.Integer, "0", "");
	public static readonly ShaderParam ALPHA_BLEND = new($"${nameof(ALPHA_BLEND)}", ShaderParamType.Integer, "0", "");
	public static readonly ShaderParam TEXTURE1 = new($"${nameof(TEXTURE1)}", ShaderParamType.Texture, "", "");
	public static readonly ShaderParam TEXTURE2 = new($"${nameof(TEXTURE2)}", ShaderParamType.Texture, "", "");
	public static readonly ShaderParam TEXTURE3 = new($"${nameof(TEXTURE3)}", ShaderParamType.Texture, "", "");
	public static readonly ShaderParam LINEARREAD_BASETEXTURE = new($"${nameof(LINEARREAD_BASETEXTURE)}", ShaderParamType.Integer, "0", "");
	public static readonly ShaderParam LINEARREAD_TEXTURE1 = new($"${nameof(LINEARREAD_TEXTURE1)}", ShaderParamType.Integer, "0", "");
	public static readonly ShaderParam LINEARREAD_TEXTURE2 = new($"${nameof(LINEARREAD_TEXTURE2)}", ShaderParamType.Integer, "0", "");
	public static readonly ShaderParam LINEARREAD_TEXTURE3 = new($"${nameof(LINEARREAD_TEXTURE3)}", ShaderParamType.Integer, "0", "");
	public static readonly ShaderParam LINEARWRITE = new($"${nameof(LINEARWRITE)}", ShaderParamType.Integer, "0", "");
	public static readonly ShaderParam COPYALPHA = new($"${nameof(COPYALPHA)}", ShaderParamType.Integer, "0", "");

	public override string? GetFallbackShader(IMaterialVar[] vars) => null;
	public override int GetFlags() => Flags;
	public override int GetNumParams() => base.GetNumParams() + ShaderParams.Count;
	public override ReadOnlySpan<char> GetParamName(int paramIndex) {
		int baseClassParamCount = base.GetNumParams();
		if (paramIndex < baseClassParamCount)
			return base.GetParamName(paramIndex);
		else
			return ShaderParams[paramIndex - baseClassParamCount].GetName();
	}
	public override ReadOnlySpan<char> GetParamHelp(int paramIndex) {
		int baseClassParamCount = base.GetNumParams();
		if (paramIndex < baseClassParamCount)
			return base.GetParamHelp(paramIndex);
		else
			return ShaderParams[paramIndex - baseClassParamCount].GetHelp();
	}
	public override ShaderParamType GetParamType(int paramIndex) {
		int baseClassParamCount = base.GetNumParams();
		if (paramIndex < baseClassParamCount)
			return base.GetParamType(paramIndex);
		else
			return ShaderParams[paramIndex - baseClassParamCount].GetType();
	}
	public override ReadOnlySpan<char> GetParamDefault(int paramIndex) {
		int baseClassParamCount = base.GetNumParams();
		if (paramIndex < baseClassParamCount)
			return base.GetParamDefault(paramIndex);
		else
			return ShaderParams[paramIndex - baseClassParamCount].GetDefaultValue();
	}

	protected override void OnInitShaderInstance(IMaterialVar[] vars, ReadOnlySpan<char> materialName) {
		if (vars[(int)ShaderMaterialVars.BaseTexture].IsDefined())
			LoadTexture((int)ShaderMaterialVars.BaseTexture);
		if (vars[TEXTURE1].IsDefined())
			LoadTexture(TEXTURE1);
		if (vars[TEXTURE2].IsDefined())
			LoadTexture(TEXTURE2);
		if (vars[TEXTURE3].IsDefined())
			LoadTexture(TEXTURE3);
	}

	void SetupSamplerSRGBRead(IMaterialVar[] vars, Sampler sampler, int textureVar, int linearReadVar) {
		ShaderShadow!.EnableTexture(sampler, true);
		ITexture txtr = vars[textureVar].GetTextureValue()!;
		ImageFormat fmt = txtr.GetImageFormat();
		if ((fmt == ImageFormat.RGBA16161616F) || (fmt == ImageFormat.RGBA16161616))
			ShaderShadow.EnableSRGBRead(sampler, false);
		else
			ShaderShadow.EnableSRGBRead(sampler, !vars[linearReadVar].IsDefined() || vars[linearReadVar].GetIntValue() == 0);
	}

	void BindTextureWithPixelSize(IShaderDynamicAPI shaderAPI, IMaterialVar[] vars, Sampler sampler, int textureVar, int constant) {
		BindTexture(sampler, textureVar, -1);

		ITexture target = vars[textureVar].GetTextureValue()!;
		Span<float> pixelSize = [1.0f / target.GetActualWidth(), 1.0f / target.GetActualHeight(), 0.0f, 0.0f];
		shaderAPI.SetPixelShaderConstant(constant, pixelSize, 1);
	}

	static ReadOnlySpan<char> PixelShaderFileName(ReadOnlySpan<char> pixelShader) {
		if (pixelShader.EndsWith("_ps20b", StringComparison.OrdinalIgnoreCase))
			return pixelShader[..^6];
		if (pixelShader.EndsWith("_ps20", StringComparison.OrdinalIgnoreCase))
			return pixelShader[..^5];
		return pixelShader;
	}

	protected override void OnDrawElements(IMaterialVar[] vars, IShaderDynamicAPI shaderAPI, VertexCompressionType vertexCompression) {
		if (IsSnapshotting()) {
			ShaderShadow.EnableDepthWrites(false);

			if (vars[(int)ShaderMaterialVars.BaseTexture].IsDefined())
				SetupSamplerSRGBRead(vars, Sampler.Sampler0, (int)ShaderMaterialVars.BaseTexture, LINEARREAD_BASETEXTURE);
			if (vars[TEXTURE1].IsDefined())
				SetupSamplerSRGBRead(vars, Sampler.Sampler1, TEXTURE1, LINEARREAD_TEXTURE1);
			if (vars[TEXTURE2].IsDefined())
				SetupSamplerSRGBRead(vars, Sampler.Sampler2, TEXTURE2, LINEARREAD_TEXTURE2);
			if (vars[TEXTURE3].IsDefined())
				SetupSamplerSRGBRead(vars, Sampler.Sampler3, TEXTURE3, LINEARREAD_TEXTURE3);

			ShaderShadow.VertexShaderVertexFormat(VertexFormat.Position, 1, null, 0);

			bool srgbWrite = true;
			if (vars[LINEARWRITE].GetFloatValue() != 0)
				srgbWrite = false;
			ShaderShadow.EnableSRGBWrite(srgbWrite);

			ShaderShadow.SetVertexShader("screenspaceeffect", 0);

			if (vars[DISABLE_COLOR_WRITES].GetIntValue() != 0)
				ShaderShadow.EnableColorWrites(false);

			ShaderShadow.EnableAlphaTest(true);
			ShaderShadow.AlphaFunc(ShaderAlphaFunc.Greater, 0.0f);

			if (IsFlagSet(vars, MaterialVarFlags.Additive))
				EnableAlphaBlending(ShaderBlendFactor.One, ShaderBlendFactor.One);
			if (vars[ALPHA_BLEND_COLOR_OVERLAY].GetIntValue() != 0)
				EnableAlphaBlending(ShaderBlendFactor.One, ShaderBlendFactor.OneMinusSrcAlpha);
			if (vars[ALPHA_BLEND].GetIntValue() != 0)
				EnableAlphaBlending(ShaderBlendFactor.SrcAlpha, ShaderBlendFactor.OneMinusSrcAlpha);

			if (vars[COPYALPHA].GetIntValue() != 0) {
				ShaderShadow.EnableBlending(false);
				ShaderShadow.AlphaFunc(ShaderAlphaFunc.Always, 0.0f);
			}

			ShaderShadow.SetPixelShader(PixelShaderFileName(vars[PIXSHADER].GetStringValue()), 0);
		}
		else {
			if (vars[(int)ShaderMaterialVars.BaseTexture].IsDefined())
				BindTextureWithPixelSize(shaderAPI, vars, Sampler.Sampler0, (int)ShaderMaterialVars.BaseTexture, 4);
			if (vars[TEXTURE1].IsDefined())
				BindTextureWithPixelSize(shaderAPI, vars, Sampler.Sampler1, TEXTURE1, 5);
			if (vars[TEXTURE2].IsDefined())
				BindTextureWithPixelSize(shaderAPI, vars, Sampler.Sampler2, TEXTURE2, 6);
			if (vars[TEXTURE3].IsDefined())
				BindTextureWithPixelSize(shaderAPI, vars, Sampler.Sampler3, TEXTURE3, 7);

			Span<float> c0 = [
				vars[C0_X].GetFloatValue(),
				vars[C0_Y].GetFloatValue(),
				vars[C0_Z].GetFloatValue(),
				vars[C0_W].GetFloatValue(),
				vars[C1_X].GetFloatValue(),
				vars[C1_Y].GetFloatValue(),
				vars[C1_Z].GetFloatValue(),
				vars[C1_W].GetFloatValue(),
				vars[C2_X].GetFloatValue(),
				vars[C2_Y].GetFloatValue(),
				vars[C2_Z].GetFloatValue(),
				vars[C2_W].GetFloatValue(),
				vars[C3_X].GetFloatValue(),
				vars[C3_Y].GetFloatValue(),
				vars[C3_Z].GetFloatValue(),
				vars[C3_W].GetFloatValue()
			];

			shaderAPI.SetPixelShaderConstant(0, c0, c0.Length / 4);

			Span<float> eyePos = [0, 0, 0, 0];
			shaderAPI.GetWorldSpaceCameraPosition(eyePos);
			shaderAPI.SetPixelShaderConstant(10, eyePos, 1);

			shaderAPI.SetVertexShaderIndex(0);
			shaderAPI.SetPixelShaderIndex(0);
		}
		Draw();
	}
}
