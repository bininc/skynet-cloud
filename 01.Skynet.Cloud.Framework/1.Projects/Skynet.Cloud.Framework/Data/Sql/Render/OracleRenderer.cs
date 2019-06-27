/*----------------------------------------------------------------
// Copyright (C) 2010 �����������Ƽ����޹�˾
// ��Ȩ���С� 
//
// �ļ�����
// �ļ�����������
//
// 
// ������ʶ��
//
// �޸ı�ʶ��
// �޸�������
//
// �޸ı�ʶ��
// �޸�������
//----------------------------------------------------------------*/
using System;
using System.Text;
using UWay.Skynet.Cloud.Linq;

namespace UWay.Skynet.Cloud.Data.Render
{
	/// <summary>
	/// Renderer for Oracle
	/// </summary>
	/// <remarks>
	/// Use OracleRenderer to render SQL statements for Oracle database.
	/// This version of Sql.Net has been tested with Oracle 9i.
	/// </remarks>
	public class OracleRenderer : SqlOmRenderer
	{

        protected override string PrefixNamed
        {
            get
            {
                return ":";
            }
        }

        /// <summary>
        /// Creates a new instance of OracleRenderer
        /// </summary>
        public OracleRenderer() : base('"', '"')
		{
			DateFormat = "dd-MMM-yy";
			DateTimeFormat = "dd-MMM-yy HH:mm:ss";
		}

		/// <summary>
		/// Renders IfNull SqlExpression
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="expr"></param>
		protected override void IfNull(StringBuilder builder, SqlExpression expr)
		{
			builder.Append("nvl(");
			Expression(builder, expr.SubExpr1);
			builder.Append(", ");
			Expression(builder, expr.SubExpr2);
			builder.Append(")");
		}

		/// <summary>
		/// Returns true. 
		/// </summary>
		protected override bool UpperCaseIdentifiers
		{
			get { return true; }
		}

		/// <summary>
		/// Renders bitwise and
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="term"></param>
		protected override void BitwiseAnd(StringBuilder builder, WhereTerm term)
		{
			builder.Append("BITAND(");
			Expression(builder, term.Expr1);
			builder.Append(", ");
			Expression(builder, term.Expr2);
			builder.Append(") > 0");
		}

		/// <summary>
		/// Renders a SELECT statement
		/// </summary>
		/// <param name="query">Query definition</param>
		/// <returns>Generated SQL statement</returns>
		public override string RenderSelect(SelectQuery query)
		{
			if (query.Top > -1 && query.OrderByTerms.Count > 0)
			{
				string baseSql = RenderSelect(query, -1);

				SelectQuery countQuery = new SelectQuery();
				SelectColumn col = new SelectColumn("*");
				countQuery.Columns.Add(col);
				countQuery.FromClause.BaseTable = FromTerm.SubQuery(baseSql, "t");
				return RenderSelect(countQuery, query.Top).Replace("\"", "").SimplifyBracket();
			}
			else
				return RenderSelect(query, query.Top).Replace("\"", "").SimplifyBracket();
		}

		string RenderSelect(SelectQuery query, int limitRows)
		{
			query.Validate();
			
			StringBuilder selectBuilder = new StringBuilder();
            //start the select
            this.Select(selectBuilder, query.IndexHints);

            //Render the Distinct statement
			this.Select(selectBuilder, query.Distinct);
			
			//Render select columns
			this.SelectColumns(selectBuilder, query.Columns);

			this.FromClause(selectBuilder, query.FromClause, query.TableSpace);

			WhereClause fullWhereClause = new WhereClause(FilterCompositionLogicalOperator.And);
            if(!query.WherePhrase.IsEmpty)
			    fullWhereClause.SubClauses.Add(query.WherePhrase);
			if (limitRows > -1)
				fullWhereClause.Terms.Add(WhereTerm.CreateCompare(SqlExpression.PseudoField("rownum"), SqlExpression.Number(limitRows), FilterOperator.IsLessThanOrEqualTo));

			this.Where(selectBuilder, fullWhereClause);
			this.WhereClause(selectBuilder, fullWhereClause);

			this.GroupBy(selectBuilder, query.GroupByTerms);
			if (query.GroupByWithCube)
				selectBuilder.Append(" cube (");
			else if (query.GroupByWithRollup)
				selectBuilder.Append(" rollup (");
			this.GroupByTerms(selectBuilder, query.GroupByTerms);

			if (query.GroupByWithCube || query.GroupByWithRollup)
				selectBuilder.Append(" )");
			
			this.Having(selectBuilder, query.HavingPhrase) ;
			this.WhereClause(selectBuilder, query.HavingPhrase);

			this.OrderBy(selectBuilder, query.OrderByTerms);
			this.OrderByTerms(selectBuilder, query.OrderByTerms);

			return selectBuilder.ToString();
		}

		/// <summary>
		/// Renders a row count SELECT statement. 
		/// </summary>
		/// <param name="query">Query definition to count rows for</param>
		/// <returns>Generated SQL statement</returns>
		/// <remarks>
		/// Renders a SQL statement which returns a result set with one row and one cell which contains the number of rows <paramref name="query"/> can generate. 
		/// The generated statement will work nicely with <see cref="System.Data.IDbCommand.ExecuteScalar"/> method.
		/// </remarks>
		public override string RenderRowCount(SelectQuery query)
		{
			string baseSql = RenderSelect(query, -1);

			SelectQuery countQuery = new SelectQuery();
			SelectColumn col = new SelectColumn("*", null, "cnt", SqlAggregationFunction.Count);
			countQuery.Columns.Add(col);
			countQuery.FromClause.BaseTable = FromTerm.SubQuery(baseSql, "t");
			return RenderSelect(countQuery).Replace("\"", "").SimplifyBracket();
		}

        /// <summary>
        /// Renders a single ORDER BY term
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="term"></param>
        protected override void OrderByTerm(StringBuilder builder, OrderByTerm term)
        {
            string dir = (term.Direction == OrderByDirection.Descending) ? "desc nulls last" : "asc nulls last";
            if (term.Exp != null)
            {
                Expression(builder, term.Exp);
            }
            else
            {
                QualifiedIdentifier(builder, term.TableAlias, term.Field);
            }
            builder.AppendFormat(" {0}", dir);
        }
	}
}