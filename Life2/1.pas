uses GraphABC;

begin
  var p := new Picture('D:\Coding\C#\Life2\1.bmp');
  var  t: text;
  assign(t,'D:\Coding\C#\Life2\1.map');
  rewrite(t);
  for var i:=0 to p.Width-1 do
  begin
  for var j:=0 to p.Height-1 do
  begin
  var c := p.Getpixel(i,j);
    if (c.R = 0)and(c.G = 0)and(c.B = 0) then
    Write(t,'*')
    else
    Write(t,' ')
  end;
  Writeln(t);
  end;
  close(t);
  Window.Close();
end.