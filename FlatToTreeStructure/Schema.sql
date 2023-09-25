create table a (id string);
create table b (id string, aid string);
create table c (id string, bid string);

insert into a values ('a1');
insert into b values ('b1', 'a1');
insert into b values ('b2', 'a1');
insert into c values ('c1', 'b1');
insert into c values ('c2', 'b1');
insert into c values ('c3', 'b2');
insert into c values ('c4', 'b2');

select *
from a, b, c;

select a.id aid, b.id bid, c.id cid
from a
inner join b on a.id = b.aid
inner join c on b.id = c.bid;

delete from c where bid = 'b2';

select a.id a_id, b.id b_id, c.id c_id
from a
left join b on a.id = b.aid
left join c on b.id = c.bid
where a.id = "a1"