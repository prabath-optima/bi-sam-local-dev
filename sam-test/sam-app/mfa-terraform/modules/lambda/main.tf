resource "aws_lambda_function" "ExtractToken" {
  filename         = "../../../src/ExtractToken/"
  function_name    = "ExtractToken"
  role             = aws_iam_role.lambda_role.arn
  handler          = "bootstrap"
  runtime          = "dotnet8"
}

resource "aws_lambda_function" "GetToken" {
  filename         = "../../../src/GetToken/"
  function_name    = "GetToken"
  role             = aws_iam_role.lambda_role.arn
  handler          = "bootstrap"
  runtime          = "dotnet8"
}
