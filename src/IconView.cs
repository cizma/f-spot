using Gtk;
using Gdk;
using Gnome;
using GtkSharp;
using System;
using System.Reflection;
using System.Collections;
using System.IO;

public class IconView : Gtk.Layout {

	// Public properties.

	/* Width of the thumbnails. */
	private int thumbnail_width = 128;
	public int ThumbnailWidth {
		get {
			return thumbnail_width;
		}

		set {
			if (thumbnail_width != value) {
				thumbnail_width = value;
				QueueResize ();
			}
		}
	}

	private double thumbnail_ratio = 4.0 / 3.0;
	public double ThumbnailRatio {
		get {
			return thumbnail_ratio;
		}
		set {
			thumbnail_ratio = value;
			QueueResize ();
		}
	}

	public int ThumbnailHeight {
		get {
			return (int) Math.Round ((double) thumbnail_width / ThumbnailRatio);
		}
	}

	// Size of the frame around the thumbnail.
	private const int CELL_BORDER_WIDTH = 15;

	// Border around the scrolled area.
	private const int BORDER_SIZE = 6;

	/* Thickness of the outline used to indicate selected items.  */
	private const int SELECTION_THICKNESS = 4;

	/* The loader.  */
	private PixbufLoader pixbuf_loader;

	/* Various other layout values.  */
	private int cells_per_row;
	private int cell_width;
	private int cell_height;

	/* Query.  */
	private PhotoQuery query;

	/* The first pixel line that is currently on the screen
	   (i.e. in the current scroll region).  Used to compute the
	   area that went offscreen in the "changed" signal handler
	   for the vertical GtkAdjustment.  */
	private int y_offset;

	/* Hash of all the order number of the items that are selected.  */
	private Hashtable selected_cells;

	/* Drag and drop bookkeeping.  */
	private bool in_drag;
	private int click_x, click_y;
	private int click_cell = -1;

	/* Number of consecutive GDK_BUTTON_PRESS on the same cell, to
	   distinguish the GDK_2BUTTON_PRESS events that we actually care
	   about.  */
	private int click_count;

	/* The pixbuf we use when we can't load a thumbnail.  */
	static Pixbuf error_pixbuf;


	// Public events.

	public delegate void DoubleClickedHandler (IconView view, int clicked_item);
	public event DoubleClickedHandler DoubleClicked;

	public delegate void SelectionChangedHandler (IconView view);
	public event SelectionChangedHandler SelectionChanged;


	// Public API.

	public IconView () : base (null, null)
	{
		pixbuf_loader = new PixbufLoader (ThumbnailWidth);
		pixbuf_loader.OnPixbufLoaded += new PixbufLoader.PixbufLoadedHandler (HandlePixbufLoaded);

		selected_cells = new Hashtable ();

		// FIXME
		// gtk_drag_dest_set (GTK_WIDGET (icon_view),
		// 		     GTK_DEST_DEFAULT_MOTION | GTK_DEST_DEFAULT_HIGHLIGHT | GTK_DEST_DEFAULT_DROP,
		//                   NULL, 0, GDK_ACTION_MOVE | GDK_ACTION_COPY);

		SizeAllocated += new SizeAllocatedHandler (HandleSizeAllocated);
		ExposeEvent += new ExposeEventHandler (HandleExposeEvent);
		ButtonPressEvent += new ButtonPressEventHandler (HandleButtonPressEvent);
		ButtonReleaseEvent += new ButtonReleaseEventHandler (HandleButtonReleaseEvent);
		MotionNotifyEvent += new MotionNotifyEventHandler (HandleMotionNotifyEvent);
	}

	private void OnReload (PhotoQuery query)
	{
		QueueResize ();
	}

	public IconView (PhotoQuery query) : this ()
	{
		this.query = query;
		query.Reload += new PhotoQuery.ReloadHandler (OnReload);
	}


	public PhotoQuery Query {
		get {
			return query;
		}
	}

	public int [] Selection {
		get {
			int [] selection = new int [selected_cells.Count];

			int i = 0;
			foreach (int cell in selected_cells.Keys)
				selection [i ++] = cell;

			Array.Sort (selection);
			return selection;
		}
	}


	// Updating.

	public void UpdateThumbnail (int thumbnail_num)
	{
		ThumbnailCache.Default.RemoveThumbnailForPath (query.Photos [thumbnail_num].DefaultVersionPath);
		QueueDraw ();
	}


	// Private utility methods.

	static private Pixbuf ErrorPixbuf ()
	{
		if (IconView.error_pixbuf == null)
			IconView.error_pixbuf = new Pixbuf (Assembly.GetCallingAssembly (), "question-mark-256.png");

		return IconView.error_pixbuf;
	}

	private int CellAtPosition (int x, int y)
	{
		if (query == null)
			return -1;

		if (x < BORDER_SIZE || x >= BORDER_SIZE + cells_per_row * cell_width)
			return -1;
		if (y < BORDER_SIZE || y >= BORDER_SIZE + (query.Photos.Length / cells_per_row + 1) * cell_height)
			return -1;

		int column = (int) ((x - BORDER_SIZE) / cell_width);
		int row = (int) ((y - BORDER_SIZE) / cell_height);
		int cell_num = column + row * cells_per_row;

		if (cell_num < query.Photos.Length)
			return (int) cell_num;
		else
			return -1;
	}

	private void UnselectAllCells ()
	{
		if (selected_cells.Count == 0)
			return;

		selected_cells.Clear ();
		QueueDraw ();

		if (SelectionChanged != null)
			SelectionChanged (this);
	}

	private bool CellIsSelected (int cell_num)
	{
		return selected_cells.ContainsKey (cell_num);
	}

	private void SelectCell (int cell_num)
	{
		if (CellIsSelected (cell_num))
			return;

		selected_cells.Add (cell_num, cell_num);

		QueueDraw ();

		if (SelectionChanged != null)
			SelectionChanged (this);
	}

	private void UnselectCell (int cell_num)
	{
		if (! CellIsSelected (cell_num))
			return;

		selected_cells.Remove (cell_num);

		QueueDraw ();

		if (SelectionChanged != null)
			SelectionChanged (this);
	}

	private void ToggleCell (int cell_num)
	{
		if (CellIsSelected (cell_num))
			UnselectCell (cell_num);
		else
			SelectCell (cell_num);
	}


	// Layout and drawing.

	private void UpdateLayout ()
	{
		int available_width = Allocation.width - 2 * BORDER_SIZE;

		cell_width = ThumbnailWidth + 2 * CELL_BORDER_WIDTH;
		cell_height = ThumbnailHeight + 2 * CELL_BORDER_WIDTH;

		cells_per_row = (int) (available_width / cell_width);
		if (cells_per_row == 0)
			cells_per_row = 1;

		int num_thumbnails;
		if (query != null)
			num_thumbnails = query.Photos.Length;
		else
			num_thumbnails = 0;

		int num_rows = num_thumbnails / cells_per_row;
		if (num_thumbnails % cells_per_row != 0)
			num_rows ++;

		SetSize ((uint) Allocation.width, (uint) (num_rows * cell_height + 2 * BORDER_SIZE));
	}

	// FIXME Cache the GCs?
	private void DrawCell (int thumbnail_num, int x, int y)
	{
		Gdk.GC gc = new Gdk.GC (BinWindow);
		gc.Copy (Style.ForegroundGC (StateType.Normal));
		gc.SetLineAttributes (1, LineStyle.Solid, CapStyle.NotLast, JoinStyle.Round);

		Photo photo = query.Photos [thumbnail_num];

		string thumbnail_path = Thumbnail.PathForUri ("file://" + photo.DefaultVersionPath, ThumbnailSize.Large);
		Pixbuf thumbnail = ThumbnailCache.Default.GetThumbnailForPath (thumbnail_path);

		Gdk.Rectangle area = new Gdk.Rectangle (x, y, cell_width, cell_height);
		Style.PaintBox (Style, BinWindow, StateType.Normal, ShadowType.Out, area, this, null, x, y, cell_width, cell_height);

		if (thumbnail == null) {
			pixbuf_loader.Request (thumbnail_path, thumbnail_num);
		} else {
			int width, height;
			PixbufUtils.Fit (thumbnail, ThumbnailWidth, ThumbnailHeight, false, out width, out height);

			Pixbuf temp_thumbnail;
			if (width == thumbnail.Width)
				temp_thumbnail = thumbnail;
			else
				temp_thumbnail = thumbnail.ScaleSimple (width, height, InterpType.Nearest);

			int dest_x = (int) (x + (cell_width - width) / 2);
			int dest_y = (int) (y + (cell_height - height) / 2);

			temp_thumbnail.RenderToDrawable (BinWindow, Style.WhiteGC,
							 0, 0, dest_x, dest_y, width, height, RgbDither.None, 0, 0);

			if (CellIsSelected (thumbnail_num)) {
				Gdk.GC selection_gc = new Gdk.GC (BinWindow);
				selection_gc.Copy (Style.BackgroundGC (StateType.Selected));
				selection_gc.SetLineAttributes (SELECTION_THICKNESS, LineStyle.Solid, CapStyle.Butt, JoinStyle.Miter);

				BinWindow.DrawRectangle (selection_gc, false,
							 dest_x - SELECTION_THICKNESS, dest_y - SELECTION_THICKNESS,
							 width + 2 * SELECTION_THICKNESS, height + 2 * SELECTION_THICKNESS);
			}
		}
	}

	private void DrawAllCells (int x, int y, int width, int height)
	{
		int start_cell_column = Math.Max ((x - BORDER_SIZE) / cell_width, 0);
		int start_cell_row = Math.Max ((y - BORDER_SIZE) / cell_height, 0);
		int start_cell_num = start_cell_column + start_cell_row * cells_per_row;

		int start_cell_x, cell_y;
		GetCellPosition (start_cell_num, out start_cell_x, out cell_y);

		int end_cell_column = Math.Max ((x + width - BORDER_SIZE) / cell_width, 0);
		int end_cell_row = Math.Max ((y + height - BORDER_SIZE) / cell_height, 0);

		int num_rows = end_cell_row - start_cell_row + 1;
		int num_cols = Math.Min (end_cell_column - start_cell_column + 1,
					 cells_per_row - start_cell_column);

		int i, cell_num;
		for (i = 0, cell_num = start_cell_num;
		     i < num_rows && cell_num < query.Photos.Length;
		     i ++) {
			int cell_x = start_cell_x;

			for (int j = 0; j < num_cols && cell_num + j < query.Photos.Length; j ++) {
				DrawCell (cell_num + j, cell_x, cell_y);
				cell_x += cell_width;
			}

			cell_y += cell_height;
			cell_num += cells_per_row;
		}
	}

	private void GetCellPosition (int cell_num, out int x, out int y)
	{
		int row = cell_num / cells_per_row;
		int col = cell_num % cells_per_row;

		x = col * cell_width + BORDER_SIZE;
		y = row * cell_height + BORDER_SIZE;
	}

	private void Scroll ()
	{
		Adjustment adjustment = Vadjustment;

		if (y_offset == adjustment.Value)
			return;

		int num_thumbnails = query.Photos.Length;
		int num_rows, start;

		if (y_offset < adjustment.Value) {
			int old_first_row = y_offset / cell_height;
			int new_first_row = (int) (adjustment.Value / cell_height);

			num_rows = new_first_row - old_first_row;
			start = old_first_row * cells_per_row;
		} else {
			int old_last_row = (y_offset + Allocation.height) / cell_height;
			int new_last_row = ((int) adjustment.Value + Allocation.height) / cell_height;

			num_rows = old_last_row - new_last_row;
			start = (new_last_row + 1) * cells_per_row;
		}

		for (int i = 0; i < cells_per_row * num_rows; i ++) {
			if (start + i >= num_thumbnails)
				break;

			pixbuf_loader.Cancel (query.Photos [start + i].DefaultVersionPath);
		}

		y_offset = (int) adjustment.Value;
	}

	private void DrawBackground (int x, int y, int width, int height)
	{
		//	/* FIXME: Is "entry_bg" the right detail value we want to pass in?  */
		//	gtk_paint_flat_box (GTK_WIDGET (view)->style, GTK_LAYOUT (view)->bin_window,
		//			    GTK_STATE_NORMAL, GTK_SHADOW_NONE, NULL,
		//			    GTK_WIDGET (view), "entry_bg",
		//			    x, y, width, height);
		// FIXME there doesn't seem to be a binding for this in C#.
	}


	// Event handlers.

	private void HandleAdjustmentValueChanged (object sender, EventArgs args)
	{
		// FIXME There doesn't seem to be a binding for this in C#.
		// if (! GTK_WIDGET_REALIZED (view))
		// return;

		Scroll ();
	}

	private void HandlePixbufLoaded (PixbufLoader loader, string path, int order, Pixbuf result)
	{
		if (result == null)
			result = ErrorPixbuf ();

		ThumbnailCache.Default.AddThumbnail (path, result);

		Rectangle area;
		GetCellPosition (order, out area.x, out area.y);
		area.width = cell_width;
		area.height = cell_height;

		DrawCell (order, area.x, area.y);

		/* (Instead of DrawCell() we could do the following instead, but it is much slower:)  */
		/* BinWindow.InvalidateRect (area, true); */
	}

	private void HandleScrollAdjustmentsSet (object sender, ScrollAdjustmentsSetArgs args)
	{
		if (args.Vadjustment != null)
			args.Vadjustment.ValueChanged += new EventHandler (HandleAdjustmentValueChanged);
	}

	private void HandleSizeAllocated (object sender, SizeAllocatedArgs args)
	{
		UpdateLayout ();
	}

	private void HandleExposeEvent (object sender, ExposeEventArgs args)
	{
		DrawBackground (args.Event.area.x, args.Event.area.y,
				args.Event.area.width, args.Event.area.height);

		DrawAllCells (args.Event.area.x, args.Event.area.y,
			      args.Event.area.width, args.Event.area.height);
	}

 	private void HandleButtonPressEvent (object obj, ButtonPressEventArgs args)
 	{
		int cell_num = CellAtPosition ((int) args.Event.x, (int) args.Event.y);

		if (cell_num < 0)
			return;

		switch (args.Event.type) {
		case EventType.TwoButtonPress:
			if (args.Event.button != 1 || click_count < 2)
				return;
			if (DoubleClicked != null)
				DoubleClicked (this, cell_num);
			return;

		case EventType.ButtonPress:
			if (args.Event.button != 1)
				return;

			if ((args.Event.state & (uint) ModifierType.ControlMask) != 0) {
				ToggleCell (cell_num);
			} else {
				UnselectAllCells ();
				SelectCell (cell_num);
			}

			// FIXME not bound in GTK#...  Sigh.
			// gdk_pointer_grab (GTK_LAYOUT (widget)->bin_window, FALSE,
			// GDK_BUTTON_RELEASE_MASK | GDK_BUTTON1_MOTION_MASK,
			//	  NULL, NULL, ev->time);

			click_x = (int) args.Event.x;
			click_y = (int) args.Event.y;

			if (click_cell == cell_num) {
				click_count ++;
			} else {
				click_cell = cell_num;
				click_count = 1;
			}

			return;

		default:
			return;
		}
	}

	private void HandleButtonReleaseEvent (object sender, ButtonReleaseEventArgs args)
	{
		// gdk_pointer_ungrab (event->time); FIXME
		in_drag = false;
	}

 	private void HandleMotionNotifyEvent (object sender, MotionNotifyEventArgs args)
 	{
		if (in_drag)
			return;

		// FIXME missing bindings.
		// if (! gtk_drag_check_threshold (widget,
		//	   			   priv->click_x, priv->click_y,
		//                                 event->x, event->y))
		//	return FALSE;

		TargetList target_list = new TargetList ();

		// FIXME missing bindings.
		// gtk_drag_begin (widget, target_list, GDK_ACTION_COPY | GDK_ACTION_MOVE, 1, (GdkEvent *) event);

		in_drag = true;
	}
}
