using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization;
using Microsoft.WindowsAzure.MediaServices.Client.DynamicEncryption;

namespace AMSUpload
{

	public static class AMSOperation
	{
		private static readonly string _accountName = "mtmedia";

		private static readonly string _accountKey = "yf5RCwPviiUJQpSoxvuGo3wa0Z6FSnF35UWV4SnsjTw=";

		private static CloudMediaContext _context = null;

		private static Uri _sampleIssuer = new Uri("http://robomateplus.com");

		private static Uri _sampleAudience = new Uri("urn:roboapp");

		private static string returnValue = "";

		private static SigningCredentials signingcredentials;

		public static bool isSuccess = false;

		private static AzureAdTokenCredentials tokenCredentials = new AzureAdTokenCredentials("mtlakshyahotmail.onmicrosoft.com", new AzureAdClientSymmetricKey("6aa0b2e9-d405-4e3d-9c7d-4b9ce8e9f1d2", "2UYq2carjTDns/f/tcEHJTa+etUOnH5NcaGcLUrQ1Ps="), AzureEnvironments.AzureCloudEnvironment);

		private static AzureAdTokenProvider tokenProvider = new AzureAdTokenProvider(tokenCredentials);

		private static void CheckConnection()
		{
			if (_context == null)
			{

				ServicePointManager.Expect100Continue = true;
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
				  | SecurityProtocolType.Tls11
				  | SecurityProtocolType.Tls12
				  | SecurityProtocolType.Ssl3
				  ;
				_context = new CloudMediaContext(new Uri("https://mtmedia.restv2.centralindia.media.azure.net/api/"), tokenProvider);
			}
		}

		public static IAsset GetAssetDetails(string path)
		{
			CheckConnection();
			return _context.Assets.Where((IAsset a) => a.Name == path.Replace(" ", "_")).FirstOrDefault();
		}

		public static string DoUpload(string path, string localPath)
		{
			try
			{
				returnValue = "";
				IAsset assetDetails = GetAssetDetails(path);
				IAccessPolicy accessPolicy = _context.AccessPolicies.Where((IAccessPolicy a) => a.Name == "365").FirstOrDefault();
				if (assetDetails == null)
				{
					IAsset asset = _context.Assets.Create(path.Replace(" ", "_"), AssetCreationOptions.None);
					ILocator locator = _context.Locators.CreateLocator(LocatorType.Sas, asset, accessPolicy);
					IAssetFile assetFile = asset.AssetFiles.Create(Path.GetFileName(localPath));
					assetFile.Upload(localPath);
					locator.Delete();
				}
				else
				{
					assetDetails.Delete(keepAzureStorageContainer: false);
					IAsset asset2 = _context.Assets.Create(path.Replace(" ", "_"), AssetCreationOptions.None);
					IAssetFile assetFile2 = asset2.AssetFiles.Create(Path.GetFileName(localPath));
					ILocator locator2 = _context.Locators.CreateLocator(LocatorType.Sas, asset2, accessPolicy);
					assetFile2.Upload(localPath);
					locator2.Delete();
				}
				isSuccess = true;
			}
			catch (Exception ex)
			{
				returnValue = "Exception : " + ex.Message + "\r\n" + ex.StackTrace;
			}
			return returnValue;
		}

		public static string DoEncoding(string path, string pCode, string basepath)
		{
			try
			{
				returnValue = "";
				isSuccess = false;
				string text = null;
				IAsset assetDetails = GetAssetDetails(path);
				IAsset assetDetails2 = GetAssetDetails(pCode);
				if (assetDetails2 != null)
				{
					assetDetails2.Delete();
					try
					{
						ServicePointManager.Expect100Continue = true;
						ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
						  | SecurityProtocolType.Tls11
						  | SecurityProtocolType.Tls12
						  | SecurityProtocolType.Ssl3
						  ;


						string requestUriString = "https://roboapi.robomateplus.in/ams/flushkey?KID=" + pCode;
						HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
						httpWebRequest.Method = "POST";
						httpWebRequest.ContentType = "text/xml";
						WebResponse response = httpWebRequest.GetResponse();
						Stream responseStream = response.GetResponseStream();
					}
					catch (Exception)
					{
					}
				}
				IAsset asset = EncodeToAdaptiveBitrateMP4Set(assetDetails, pCode, basepath);
				IContentKey contentKey = CreateEnvelopeTypeContentKey(asset);
				_sampleAudience = new Uri("urn:" + pCode);
				text = AddTokenRestrictedAuthorizationPolicy(contentKey);
				CreateAssetDeliveryPolicy(asset, contentKey);
				if (!string.IsNullOrEmpty(text))
				{
					TokenRestrictionTemplate tokenTemplate = TokenRestrictionTemplateSerializer.Deserialize(text);
					Guid keyIdAsGuid = EncryptionUtils.GetKeyIdAsGuid(contentKey.Id);
					string text2 = TokenRestrictionTemplateSerializer.GenerateTestToken(tokenTemplate, null, keyIdAsGuid);
					returnValue = returnValue + "The authorization token is:\nBearer " + text2 + "\r\n";
					Console.WriteLine();
				}
				string streamingOriginLocator = GetStreamingOriginLocator(asset);
				returnValue = returnValue + "Smooth Streaming Url: " + streamingOriginLocator + "/manifest\r\n";
				returnValue = returnValue + "HLS Url: " + streamingOriginLocator + "/manifest(format=m3u8-aapl)\r\n";
				isSuccess = true;
			}
			catch (Exception ex2)
			{
				returnValue = "Exception : " + ex2.Message + "\r\n" + ex2.StackTrace;
			}
			return returnValue;
		}

		public static IAsset EncodeToAdaptiveBitrateMP4Set(IAsset asset, string pCode, string basepath)
		{
			IJob job = _context.Jobs.Create("EncoderJob :: " + pCode);
			IMediaProcessor latestMediaProcessorByName = GetLatestMediaProcessorByName("Media Encoder Standard");
			MyMediaInfo mediaInfo = Utility.GetMediaInfo(basepath);
			string newValue = Convert.ToString(mediaInfo.V.Bitrate / 1000);
			int heigth = mediaInfo.V.Heigth;
			int width = mediaInfo.V.Width;
			string text = File.ReadAllText("CustomPreset_JSON.json");
			int num = 96000;
			text = text.Replace("mbitRate", newValue);
			text = text.Replace("mHeight", heigth.ToString());
			text = text.Replace("mWidth", width.ToString());
			text = text.Replace("abitRate", Convert.ToString(num / 1000));
			ITask task = job.Tasks.AddNew("EncodingTask :: " + pCode, latestMediaProcessorByName, text, TaskOptions.None);
			task.InputAssets.Add(asset);
			task.OutputAssets.AddNew(pCode, AssetCreationOptions.None);
			job.StateChanged += JobStateChanged;
			job.Submit();
			job.GetExecutionProgressTask(CancellationToken.None).Wait();
			return job.OutputMediaAssets[0];
		}

		private static IMediaProcessor GetLatestMediaProcessorByName(string mediaProcessorName)
		{
			IMediaProcessor mediaProcessor = (from p in _context.MediaProcessors.Where((IMediaProcessor p) => p.Name == mediaProcessorName).ToList()
											  orderby new Version(p.Version)
											  select p).LastOrDefault();
			if (mediaProcessor == null)
			{
				throw new ArgumentException(string.Format("Unknown media processor", mediaProcessorName));
			}
			return mediaProcessor;
		}

		public static IContentKey CreateEnvelopeTypeContentKey(IAsset asset)
		{
			Guid keyId = Guid.NewGuid();
			byte[] randomBuffer = GetRandomBuffer(16);
			IContentKey contentKey = _context.ContentKeys.Create(keyId, randomBuffer, "ContentKey", ContentKeyType.EnvelopeEncryption);
			asset.ContentKeys.Add(contentKey);
			return contentKey;
		}

		public static string AddTokenRestrictedAuthorizationPolicy(IContentKey contentKey)
		{
			string text = GenerateTokenRequirements();
			IContentKeyAuthorizationPolicy result = _context.ContentKeyAuthorizationPolicies.CreateAsync("HLS token restricted authorization policy").Result;
			List<ContentKeyAuthorizationPolicyRestriction> list = new List<ContentKeyAuthorizationPolicyRestriction>();
			ContentKeyAuthorizationPolicyRestriction item = new ContentKeyAuthorizationPolicyRestriction
			{
				Name = "Token Authorization Policy",
				KeyRestrictionType = 1,
				Requirements = text
			};
			list.Add(item);
			IContentKeyAuthorizationPolicyOption item2 = _context.ContentKeyAuthorizationPolicyOptions.Create("Token option for HLS", ContentKeyDeliveryType.BaselineHttp, list, null);
			result.Options.Add(item2);
			contentKey.AuthorizationPolicyId = result.Id;
			IContentKey result2 = contentKey.UpdateAsync().Result;
			returnValue = returnValue + "Adding Key to Asset: Key ID is " + result2.Id + "\r\n";
			return text;
		}

		public static void CreateAssetDeliveryPolicy(IAsset asset, IContentKey key)
		{
			Uri keyDeliveryUrl = key.GetKeyDeliveryUrl(ContentKeyDeliveryType.BaselineHttp);
			string text = Convert.ToBase64String(GetRandomBuffer(16));
			Dictionary<AssetDeliveryPolicyConfigurationKey, string> configuration = new Dictionary<AssetDeliveryPolicyConfigurationKey, string> {
		{
			AssetDeliveryPolicyConfigurationKey.EnvelopeKeyAcquisitionUrl,
			keyDeliveryUrl.ToString()
		} };
			IAssetDeliveryPolicy assetDeliveryPolicy = _context.AssetDeliveryPolicies.Create("AssetDeliveryPolicy", AssetDeliveryPolicyType.DynamicEnvelopeEncryption, AssetDeliveryProtocol.SmoothStreaming | AssetDeliveryProtocol.Dash | AssetDeliveryProtocol.HLS, configuration);
			asset.DeliveryPolicies.Add(assetDeliveryPolicy);
			returnValue = string.Concat(returnValue, "Adding Asset Delivery Policy: ", assetDeliveryPolicy.AssetDeliveryPolicyType, "\r\n");
		}

		private static string GenerateTokenRequirements()
		{
			TokenRestrictionTemplate tokenRestrictionTemplate = new TokenRestrictionTemplate(TokenType.JWT);
			tokenRestrictionTemplate.PrimaryVerificationKey = new SymmetricVerificationKey();
			tokenRestrictionTemplate.AlternateVerificationKeys.Add(new SymmetricVerificationKey());
			tokenRestrictionTemplate.Audience = _sampleAudience.ToString();
			tokenRestrictionTemplate.Issuer = _sampleIssuer.ToString();
			tokenRestrictionTemplate.RequiredClaims.Add(TokenClaim.ContentKeyIdentifierClaim);
			return TokenRestrictionTemplateSerializer.Serialize(tokenRestrictionTemplate);
		}

		public static string GetStreamingOriginLocator(IAsset asset)
		{
			IAssetFile assetFile = asset.AssetFiles.Where((IAssetFile f) => f.Name.ToLower().EndsWith(".ism")).FirstOrDefault();
			IAccessPolicy accessPolicy = _context.AccessPolicies.Create("Streaming policy", TimeSpan.FromDays(365.0), AccessPermissions.Read);
			ILocator locator = _context.Locators.CreateLocator(LocatorType.OnDemandOrigin, asset, accessPolicy, DateTime.UtcNow.AddMinutes(-5.0));
			return locator.Path + assetFile.Name;
		}

		private static void JobStateChanged(object sender, JobStateChangedEventArgs e)
		{
			returnValue += string.Format("{0}\n  State: {1}\n  Time: {2}\r\n", ((IJob)sender).Name, e.CurrentState, DateTime.UtcNow.ToString("yyyy_M_d__hh_mm_ss"));
		}

		private static byte[] GetRandomBuffer(int size)
		{
			byte[] array = new byte[size];
			using (RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider())
			{
				rNGCryptoServiceProvider.GetBytes(array);
			}
			return array;
		}

		public static string GetToken(IAsset MyAsset, string Issuer, string Audiance)
		{
			string text = "";
			IContentKey key = MyAsset.ContentKeys.Where((IContentKey k) => k.ContentKeyType == ContentKeyType.EnvelopeEncryption).FirstOrDefault();
			if (key != null && key.AuthorizationPolicyId != null)
			{
				IContentKeyAuthorizationPolicy contentKeyAuthorizationPolicy = _context.ContentKeyAuthorizationPolicies.Where((IContentKeyAuthorizationPolicy p) => p.Id == key.AuthorizationPolicyId).FirstOrDefault();
				if (contentKeyAuthorizationPolicy != null)
				{
					IContentKeyAuthorizationPolicyOption contentKeyAuthorizationPolicyOption = null;
					contentKeyAuthorizationPolicyOption = contentKeyAuthorizationPolicy.Options.Where((IContentKeyAuthorizationPolicyOption o) => o.Restrictions.FirstOrDefault().KeyRestrictionType == 1).FirstOrDefault();
					if (contentKeyAuthorizationPolicyOption != null)
					{
						string requirements = contentKeyAuthorizationPolicyOption.Restrictions.FirstOrDefault().Requirements;
						if (!string.IsNullOrEmpty(requirements))
						{
							Guid keyIdAsGuid = EncryptionUtils.GetKeyIdAsGuid(key.Id);
							TokenRestrictionTemplate tokenRestrictionTemplate = TokenRestrictionTemplateSerializer.Deserialize(requirements);
							if (tokenRestrictionTemplate.OpenIdConnectDiscoveryDocument == null)
							{
								List<Claim> list = null;
								list = new List<Claim>
							{
								new Claim(TokenClaim.ContentKeyIdentifierClaimType, keyIdAsGuid.ToString())
							};
								if (tokenRestrictionTemplate.PrimaryVerificationKey.GetType() == typeof(SymmetricVerificationKey))
								{
									InMemorySymmetricSecurityKey signingKey = new InMemorySymmetricSecurityKey((tokenRestrictionTemplate.PrimaryVerificationKey as SymmetricVerificationKey).KeyValue);
									signingcredentials = new SigningCredentials(signingKey, "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256", "http://www.w3.org/2001/04/xmlenc#sha256");
								}
								string issuer = tokenRestrictionTemplate.Issuer;
								string audience = tokenRestrictionTemplate.Audience;
								DateTime? notBefore = DateTime.Now.AddMinutes(-5.0);
								DateTime? expires = DateTime.Now.AddMinutes(1.0);
								JwtSecurityToken token = new JwtSecurityToken(signingCredentials: signingcredentials, issuer: issuer, audience: audience, claims: list, notBefore: notBefore, expires: expires);
								JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
								text = jwtSecurityTokenHandler.WriteToken(token);
							}
						}
					}
				}
			}
			returnValue = "The authorization token is:\nBearer " + text + "\r\n";
			return returnValue;
		}
	}
}