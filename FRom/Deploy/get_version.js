//�������� ��� � �����, ������ �������� ���������� �������
var args = WScript.Arguments;
var fileName = args.Item(0);
//������ ��� ������ � �������� ��������
var fso = new ActiveXObject("Scripting.FileSystemObject");
//������ �����
var fileVersion = fso.GetFileVersion(fileName);
//����� � outstream
WScript.Echo(fileVersion);