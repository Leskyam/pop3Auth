using System;
using System.Web;
using System.Net.Sockets;
using System.Web.Services;
using System.Web.Services.Protocols;

namespace pop3Auth
{
	/// <summary>
	/// Clase a través de la cual se implementa la devolución de objetos de tipo Exception.
	/// </summary>
	public class pop3Exception : System.ApplicationException
	{

		public pop3Exception(string str):base(str)
		{
     
		}
	}

	/// <summary>
	/// Implementación de sistema de identificación y autorización de usuarios a través de servidores POP3.
	/// </summary>
	[WebService(Namespace="http://webservices.cf.minaz.cu/")]
	public class pop3Auth : System.Net.Sockets.TcpClient //System.Web.Services.WebService
	{
		public pop3Auth()
		{
			//CODEGEN: llamada necesaria para el Diseñador de servicios Web ASP .NET
			//InitializeComponent();
		}

		//
		#region "PROCEDIMIENTOS PRIVADOS"

		private string getResponse()
		{
			System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
			byte[] serverBuffer = new byte[1024];
			NetworkStream netStream = this.GetStream();
			int counter = 0;
			while (true)
			{
				byte[] buff = new byte[2];
				int bytes = netStream.Read(buff, 0, 1);
				if (bytes == 1)
				{
					serverBuffer[counter] = buff[0];
					counter++;

					if (buff[0] == '\n')
					{
						break;
					}
				}
				else
				{
					break;
				}
			}

			string retVal = enc.GetString(serverBuffer, 0, counter);
			System.Diagnostics.Debug.WriteLine("READ: " + retVal);
			return retVal; 
		}

		private void Write(string mensaje) 
		{
			System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
			byte[] writeBuffer = new byte[1024];
			writeBuffer = enc.GetBytes(mensaje);

			NetworkStream netStream = this.GetStream();
			netStream.Write(writeBuffer, 0, writeBuffer.Length);
    
			System.Diagnostics.Debug.WriteLine("WRITE: " + mensaje);
		}

		private void Logout()
		{
			string mensaje, respuesta;
			mensaje = "QUIT\r\n";
			this.Write(mensaje);
			respuesta = getResponse();
			if (respuesta.Substring(0, 3) != "+OK")
			{
				throw new pop3Exception(respuesta); 
			}
		}

		private bool LogonToServer(string POP3Server, int Port, string userEmail, string userPassword)
		{
			string mensaje, respuesta;

			try
			{
				this.Connect(POP3Server, Port);
			}
			catch (System.Exception Ex)
			{
				throw new pop3Exception(Ex.Message);
			}
    
			respuesta = getResponse();
			if (respuesta.Substring(0, 3) != "+OK")
			{
				throw new pop3Exception(respuesta);
			}

			mensaje = "USER " + userEmail + "\r\n";
			this.Write(mensaje);
			respuesta = getResponse();
			if (respuesta.Substring(0, 3) != "+OK")
			{
				throw new pop3Exception(respuesta);
			}

			mensaje = "PASS " + userPassword + "\r\n";
			this.Write(mensaje);
			respuesta = getResponse();
			if (respuesta.Substring(0, 3) != "+OK")
			{
				return false;
				//throw new pop3Exception(respuesta);
			}

			Logout();
			return true;
		}

		#endregion
  
		#region "MÉTODOS WEB (Públicos)"

		[System.Web.Services.WebMethod(MessageName="LogonToSpecificServer",Description="Permite que se evalúen las credenciales " +
			 "pasadas como valores de los correspondientes parámetros (userEmail y userPassword) contra el servidor POP3 cuyo " + 
			 "nombre o dirección IP se pasa como valor del parámetro 'pop3Server', y haciendo uso del puerto que se especifique " + 
			 "en el valor del parámetro 'Port'.")]
		public bool Logon(string POP3Server, int Port, string userEmail, string userPassword)
		{
      return this.LogonToServer(POP3Server, Port, userEmail, userPassword);
		}

		[System.Web.Services.WebMethod(MessageName="LogonToServersInConfig",Description="Permite que se evalúen las credenciales " + 
			 "pasadas como valores de los correspondientes parámetros (userEmail y userPassword) contra todos los servidores POP3 " +
			 "configurados en la llave 'pop3Servers' de la sección 'appSettings' del fichero de configuración 'Web.config' de " + 
			 "este servicio.")]
		public bool Logon(string userEmail, string userPassword)
		{
			clsSettings settings = new clsSettings();
			clsSettings.popServer[] pop3Servers;

			try
			{
				pop3Servers = settings.getPOP3Servers();
			}
			catch(System.Exception Ex)
			{
				throw new System.Exception(Ex.Message);
			}
			
			for(int x=0; x<=pop3Servers.Length-1; x++)
			{
				try
				{
					if(this.LogonToServer(pop3Servers[x].name, pop3Servers[x].port, userEmail, userPassword))
					{
						return true;
					}
				}
				catch(pop3Exception)
				{
					//          strErrorMessageByServer +=	pop3Servers[x].name.ToUpper() + 
					//																			", Puerto: " + pop3Servers[x].port + 
					//																			", Mensaje: " + Ex.Message + "\r\n";
          continue;
				}
			}

			//			System.Diagnostics.Debug.WriteLine(strErrorMessageByServer); 
			//			throw new System.ApplicationException(strErrorMessageByServer);
			return false;

		}
		#endregion

	}
}
