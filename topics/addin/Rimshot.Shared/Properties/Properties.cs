namespace Rimshot.Shared.Properties {

  /*
  internal class PersistedProperties<T> {

    private IPropertyComparer<T> m_compare;
    private List<PersistedPropertyCategories<T>> m_by_category;

    public PersistedProperties ( IPropertyComparer<T> compare ) {
      this.m_compare = compare;
      this.m_by_category = new List<PersistedPropertyCategories<T>>();
    }
  }

  internal class PersistedPropertyCategories<T> {
    public T CategoryName { get; private set; }

    public List<T> PropertyNames { get; private set; }
  }

  internal interface IPropertyComparer<T> : IEqualityComparer<T> {
    bool Equals ( T x, CategoryVM cat );
    bool Equals ( T x, IPropertyVM prop );
    bool Equals ( T x, string name );
  }

  public class CategoryVM {
    public string DisplayName { get; private set; }

    public string Name { get; protected set; }

    public string InternalName { get; protected set; }

    public IEnumerable<IPropertyVM> Properties { get; protected set; }

    public CategoryVM () {
    }

    public CategoryVM ( PropertyCategory pc ) {
      this.Name = pc.DisplayName;
      this.InternalName = pc.Name;
      this.DisplayName = this.Name;
      this.Properties = ( IEnumerable<IPropertyVM> )( ( IEnumerable<DataProperty> )pc.Properties ).Select<DataProperty, PropertyVM>( ( Func<DataProperty, PropertyVM> )( p => new PropertyVM( this, p ) ) ).ToList<PropertyVM>();
    }
  }

  public interface IPropertyVM {
    string CategoryName { get; }
    string CategoryDisplayName { get; }
    string Name { get; }
    string DisplayName { get; }
    string ValueStr { get; }
    StringProperty Value { get; }
  }

  public class StringProperty {
    public string Text { get; set; }
    public StringProperty ( string value ) => this.Text = value;
  }

  public class PropertyVM : IPropertyVM {
    private VariantData m_data;
    private StringProperty m_value;
    public string CategoryName { get; private set; }
    public string CategoryDisplayName { get; private set; }
    public string Name { get; private set; }
    public string DisplayName { get; private set; }
    public string ValueStr => this.Value?.Text;
    public StringProperty Value {
      get {
        if ( this.m_value == null ) {
          BuildValue();
        }

        return this.m_value;
      }
    }
    public PropertyVM ( CategoryVM category, DataProperty p ) {
      this.CategoryName = category.Name;
      this.CategoryDisplayName = category.DisplayName;
      this.Name = p.DisplayName;
      this.m_data = p.Value;
      this.DisplayName = this.Name;
    }

    private void BuildValue () {
      string s;
      LcOaData.ConvertAndPrettyPrint( this.m_data, Application.MainDocument.Units, ( LcOaUnitAngular )1, ref s, true );
      if ( s == null ) {
        s = "";
      } else {
        try {
          if ( this.m_data.IsDisplayString ) {
            if ( IsHyperlink( s ) ) {
              this.m_value = ( StringProperty )new UriProperty( s );
            }
          }
        } catch {
        }
      }
      if ( this.m_value != null )
        return;
      this.m_value = new StringProperty( s );
    }

    private bool IsHyperlink ( string s ) => s.StartsWith( "http://" ) || s.StartsWith( "https://" ) || s.StartsWith( "www." ) || s.StartsWith( "mailto:" ) || s.StartsWith( "file://" ) || File.Exists( s );
  }

  public class UriProperty : StringProperty {
    public Uri Link { get; set; }

    public UriProperty ( string value )
      : base( value ) {
      this.Link = new Uri( value );
    }
  }

  */
}
