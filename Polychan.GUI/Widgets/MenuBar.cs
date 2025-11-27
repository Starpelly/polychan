using SkiaSharp;

namespace Polychan.GUI.Widgets
{
    public class MenuBar : Widget, IPaintHandler, IMouseDownHandler
    {
        public enum Orientation
        {
            Top,
            Left,
            Right,
            Bottom
        }

        public Orientation ScreenPosition { get; set; }

        private const int MENU_BAR_HEIGHT = 24;
        private const int BORDER_SIZE = 1;
        private const bool DRAW_BORDER = BORDER_SIZE > 0;

        private int m_nextX = 0;

        private readonly List<Menu> m_menus = [];

        private Menu? m_openedMenu = null;

        private readonly MenuPopup m_stdPopup;
        private bool m_popupOpen = false;

        public MenuBar(Widget? parent = null) : base(parent)
        {
            Height = MENU_BAR_HEIGHT + BORDER_SIZE;
            m_stdPopup = new MenuPopup(this);
            m_stdPopup.OnSubmitted = () =>
            {
                m_stdPopup.Hide();
                m_openedMenu?.Close();

                m_openedMenu = null;
                m_popupOpen = false;
            };
        }

        private void AddMenu(Menu menu)
        {
            // @NOTE - pelly, 11/26/25
            // This is FINE, shouldn't throw an exception.
            // if (m_menus.Contains(menu))
            //    throw new Exception("Please don't add the same menu multiple times!");

            menu.SetPosition(m_nextX, 0);
            menu.Height = MENU_BAR_HEIGHT;
            
            menu.OnHovered = () =>
            {
                if (m_popupOpen && m_openedMenu != menu)
                {
                    m_openedMenu?.Close();
                    menu.Open();
                }
            };
            menu.OnUserOpened = () =>
            {
                if (m_openedMenu == menu)
                    return;

                m_openedMenu = menu;
                m_popupOpen = true;

                menu.Open();
                m_stdPopup.SetMenu(menu);
                m_stdPopup.SetPosition(menu.X, menu.Height);
                m_stdPopup.Show();
            };
            menu.OnUserClosed = () =>
            {
                if (m_openedMenu != menu) return;

                m_openedMenu = null;
                m_popupOpen = false;

                m_stdPopup.SetMenu(null);
                m_stdPopup.Hide();
            };

            m_menus.Add(menu);
            
            m_nextX += menu.Width;
        }

        public Menu AddMenu(string title)
        {
            var t = new Menu(new MenuAction(title), Menu.MenuItemType.MenuBarSubMenu, this);
            AddMenu(t);
            return t;
        }

        public Menu AddMenu(string icon, string title)
        {
            var t = new Menu(new MenuAction(icon, title), Menu.MenuItemType.MenuBarSubMenu, this);
            AddMenu(t);
            return t;
        }

        public void OnPaint(SKCanvas canvas)
        {
            using var paint = new SKPaint();

            if (DRAW_BORDER)
            {
                paint.Color = Application.DefaultStyle.GetFrameColor();
                canvas.DrawRect(0, 0, Width, Height, paint);
            }

            paint.Color = EffectivePalette.Get(ColorRole.Window);
            canvas.DrawRect(0, 0, Width, Height - BORDER_SIZE, paint);
        }

        public bool OnMouseDown(MouseEvent evt)
        {
            return true;
        }

        /// <summary>
        /// Used to prevent a "double open" when clicking outside a popup but on the menubar...
        /// This seems kinda hacky...
        /// </summary>
        internal bool OwnsPopup(MenuPopup? popup)
        {
            if (m_stdPopup == popup)
                return true;

            return false;
        }
    }
}