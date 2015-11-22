using System;

namespace pop3Auth
{
	public class clsSettings
	{
		public clsSettings()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		/// <summary>
		/// Estructura que se emplear� para representar cada servidor que se configure
		/// en el fichero Web.config de este servicio.
		/// </summary>
		public struct popServer
		{
			public string name;
			public int port;
		}
		/// <summary>
		/// Devuelve del web.config el valor de la llave (key) que se pase como par�metro.
		/// </summary>
		/// <param name="strKey">Nombre de la llave de la cual se debe recuperar el valor</param>
		/// <returns>El valor de la llave que se pasa como par�metro</returns>
		private static string getWebConfigSimpleValue(string strKeyName)
		{
			if (System.Configuration.ConfigurationSettings.AppSettings[strKeyName] != null)
			{
				return System.Configuration.ConfigurationSettings.AppSettings[strKeyName];
			}
			else
			{
				throw new System.Configuration.ConfigurationException("La llave '" + strKeyName + "' no existe en la secci�n 'appSettings' del fichero de configuraci�n 'web.config'");
			}
		}

    /// <summary>
    /// Obtiene un arreglo de los servidores POP3 que se encuentren configurados en el Web.config
    /// </summary>
    /// <returns>Arreglo de la estructura 'popServer'</returns>
		public popServer[] getPOP3Servers()
		{
			string strServers = getWebConfigSimpleValue("pop3Servers");
			if(strServers == string.Empty)
			{
				throw new System.Configuration.ConfigurationException("El valor de la llave 'pop3Servers' en el fichero Web.config no puede estar vac�o en caso de aparecer.");
			}
			string[] aryServers = strServers.Split('|');
			popServer[] pop3Servers = new popServer[aryServers.Length];

			try
			{
				for(int x=0; x<=(aryServers.Length-1); x++)
				{
					pop3Servers[x].name = aryServers[x].Substring(0,aryServers[x].IndexOf(":"));
					pop3Servers[x].port = System.Convert.ToInt16(aryServers[x].Substring(aryServers[x].IndexOf(":")+1));
				}
			}
			catch(System.Exception Ex)
			{
				System.Diagnostics.Debug.WriteLine(Ex.ToString());
				throw new System.Configuration.ConfigurationException("Revisar los valores servidor POP y puerto " + 
					"que se encuentran en la en el fichero 'Web.config'");
			}

			return pop3Servers;
		}

	}
}
