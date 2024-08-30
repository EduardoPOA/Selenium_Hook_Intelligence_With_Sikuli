/*
 * @author Eduardo Oliveira
 */
using System;
using System.Drawing;

namespace Hook_Validator.Rest
{
    /// <summary>
    /// Descrição do Pattern.
    /// </summary>
    public class Pattern
	{
		/// <summary>
		/// O caminho da imagem para executar as ações.
		/// </summary>
		public String ImagePath {get; set;}
		/// <summary>
		/// O deslocamento do destino da imagem, partindo do centro como 0,0 que é o padrão.
		/// </summary>
		public Point Offset {get; set;}
		/// <summary>
		/// A semelhança percentual que a ferramenta procura ao pesquisar o padrão 0.7 é default (70%)
		/// </summary>
		public Double Similar {get; set;}
		
		public Pattern() : this("", new Point(0,0), 0.7) {}
		public Pattern(String imagePath) : this(imagePath, new Point(0,0), 0.7) {}
		public Pattern(String imagePath, Point offset) : this(imagePath, offset, 0.7) {}
		public Pattern(String imagePath, Double similar) : this(imagePath, new Point(0,0), similar) {}
		/// <summary>
		/// Instancia uma nova instância do objeto Padrão, a ser usado pela ferramenta para encontrar a imagem especificada na tela.
		/// </summary>
		/// <param name="imagePath">O caminho para a imagem usada neste objeto padrão; geralmente estará no formato .png</param>
		/// <param name="offset">O deslocamento de destino da imagem, indo do centro em 0,0, que é o padrão</param>
		/// <param name="similar">A porcentagem de similaridade que a ferramenta procura ao pesquisar o padrão. 0,7 é o padrão (70%)</param>
		public Pattern(String imagePath, Point offset, Double similar)
		{
			ImagePath = imagePath;
			Offset = offset;
			Similar = similar;
		}
		/// <summary>
		/// Método para obter o objeto json_Pattern que se correlaciona com este objeto Padrão. 
		/// Para ser utilizado pela ferramenta no repasse de informações ao serviço.
		/// </summary>
		/// <returns></returns>
		public Json.json_Pattern ToJsonPattern()
		{
			Json.json_Pattern jPattern = new Hook_Validator.Json.json_Pattern();
			jPattern.imagePath = this.ImagePath;
			jPattern.offset_x = this.Offset.X;
			jPattern.offset_y = this.Offset.Y;
			jPattern.similar = (float)this.Similar;
			return jPattern;
		}
	}
}
