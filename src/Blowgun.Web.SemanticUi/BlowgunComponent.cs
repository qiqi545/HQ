using Blowdart.UI.Web;

namespace Blowgun.Web.SemanticUi
{
    public abstract class BlowgunComponent : HtmlComponent
    {
        protected BlowgunComponent() : base()
        {

        }
    }

    public abstract class BlowgunComponent<T> : HtmlComponent<T>
    {

    }
}
