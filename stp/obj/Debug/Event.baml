   M S B A M L   `   `   `  ����>  :stp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:stpstp  5   	       	stp.Event   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   X TXceed.Wpf.Toolkit, Version=3.3.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f44xctk)http://schemas.xceed.com/wpf/xaml/toolkit  5   	   localclr-namespace:stp  5   	     � Title$  Event��5	   	   $	��494��6   $	��482��6$   .��5
      � $��10,0,0,0q�6   .+�5   
   �� $
��Date��6   $
��Left=�6   $��	58,23,0,0q�6:   $	��Top=�6M   &   Xceed.Wpf.Toolkit.DateTimePicker  5   
   -   5          Name$ 	DateField��5      &   Xceed.Wpf.Toolkit.DateTimeUpDown   Format$ UniversalSortableDateTime=�61    � Language$ en-br�6T    �� Comments$ en-br��6e    �� 
Attributes$ en-br��6�   $
��Left=�6�   $��	98,23,0,0q�6�   $	��Top=�6�   $	��340��6�   ,  &Xceed.Wpf.Toolkit.Primitives.InputBase   CultureInfo$ en-AU��6�   $��26��6	  �� 5   
   $
��Name��6   $
��Left=�6   $��	50,51,0,0q�6:   $	��Top=�6M   �� 5   
   -   5         ��Name$ 	NameField��5      $
��Left=�6%   $��23��6@   $��	98,54,0,0q�6L    �� TextWrapping$
 Wrap=�6_   $	��Top=�6s   $	��340��6�   �� 5   
   $��Place��6   $
��Left=�6    $��	54,79,0,0q�6;   $	��Top=�6N   �� 5   
   -   5        $ 
PlaceField��5      $
��Left=�6&   $��23��6A   $��	98,82,0,0q�6M   $
 Wrap=�6`   $	��Top=�6t   $	��340��6�   �� 5   
   $��Description��6   $
��Left=�6&   $��
22,107,0,0q�6A   $	��Top=�6U   �� 5   
   -   5        $ DescrioptionField��5      $
��Left=�6-   $	��117��6H   $��
98,110,0,0q�6U   $
 Wrap=�6i   $	��Top=�6}   $	��340��6�   �� 5   
   $��Duration��6   $
��Left=�6#   $��
36,229,0,0q�6>   $	��Top=�6R   �� 5   
   -   5        $ DurationField��5      $
��Left=�6)   $��23��6D   $��
98,232,0,0q�6P   $
 Wrap=�6d   $	��Top=�6x   $	��340��6�   �� 5   
   $��Notify before��6   $
��Left=�6(   $��
12,257,0,0q�6C   $	��Top=�6W   �� 5   
   -   5        $ NotifyBeforeField��5      $
��Left=�6-   $��23��6H   $��
98,260,0,0q�6T   $
 Wrap=�6h   $	��Top=�6|   $	��340��6�   �� 5   
   $��Guests��6   $
��Left=�6!   $��
46,288,0,0q�6<   $	��Top=�6P   �� 5   
   -   5        	 ��Name$	 GuestsListView��5      $
��Left=�6*   $	��132��6E   $��
98,288,0,0q�6R   $	��Top=�6f   $	��340��6~   ��6�   �� ��5      �� 5      .R�5      � $	��0,2q�6   
 � ColumnDefinitions
 5      �� 5      $��30 �6/   6<   �� 5      $	��100 �6/   6=   �� 5       $	��200 �6/   6=   .+�5"      ��  d� 	IsChecked 6#   ��  �� Path$ Optional��� 5#      $��1��6$   ��64   �� $ ResponseStatus��� 5$      $��2��6$   ��64   �� $ Email�5%      5&      5(      �� 5*   
   $y�Hidden=�6   $	��Add��6%   $
��Left=�63   $��192,425,0,0q�6N   $	��Top=�6c   $��74��6{   �� 5+   
   $y�Hidden=�6   $
��Save��6%   $
��Left=�64   $��286,425,0,0q�6O   $	��Top=�6d   $��74��6|   �� 5,   
   $y�Hidden=�6   $��Remove��6%   $
��Left=�66   $��
98,425,0,0q�6Q   $	��Top=�6e   $��74��6}   5-      5.      